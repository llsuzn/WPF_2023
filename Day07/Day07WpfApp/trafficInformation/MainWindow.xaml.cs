using Google.Protobuf.WellKnownTypes;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using trafficInformation.Logics;
using trafficInformation.Models;
using MySql.Data.MySqlClient;
using MahApps.Metro.Behaviors;
using System.Threading;
using CefSharp.Wpf;
using CefSharp;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;

namespace trafficInformation
{    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        LibVLC _libVLC;
        LibVLCSharp.Shared.MediaPlayer _mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();

            Core.Initialize();

            _libVLC = new LibVLC();
            _mediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);

            CCTVSCREEN.MediaPlayer = _mediaPlayer;

            do
            {
                Reload();
            } while (false);

        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtFindInfo.Focus();
        }

        private void TxtFindInfo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearch_Click(sender, e);
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFindInfo.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 데이터를 입력하세요.");
                return;
            }
            try
            {
                SearchInfo(TxtFindInfo.Text);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류발생 {ex.Message}");
            }
        }

        private async void SearchInfo(string areatext)
        {
            this.DataContext = null;
            TxtFindInfo.Text = string.Empty;

            List<TrafficInfo> list = new List<TrafficInfo>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = $@"SELECT * FROM miniproject.trafficinfo
		                        WHERE Cctvname LIKE '%{areatext}%';";
                    var cmd = new MySqlCommand(query, conn);
                    var adapter = new MySqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "TrafficInfo");

                    foreach (DataRow dr in dSet.Tables["TrafficInfo"].Rows)
                    {
                        list.Add(new TrafficInfo
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Datacount = Convert.ToInt32(dr["Datacount"]),
                            Roadsectionid = Convert.ToString(dr["Roadsectionid"]),
                            Filecreatetime = Convert.ToString(dr["Filecreatetime"]),
                            Cctvtype = Convert.ToString(dr["Cctvtype"]),
                            Cctvurl = Convert.ToString(dr["Cctvurl"]),
                            Cctvresolution = Convert.ToString(dr["Cctvresolution"]),
                            Coordx = Convert.ToString(dr["Coordx"]),
                            Coordy = Convert.ToString(dr["Coordy"]),
                            Cctvformat = Convert.ToString(dr["Cctvformat"]),
                            Cctvname = Convert.ToString(dr["Cctvname"])
                        });
                    }
                    this.DataContext = list;
                    StsResult.Content = $"데이터 {list.Count}건 조회완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB조회 오류 {ex.Message}");
            }
        }

        private async void GrdResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string cctvPath = string.Empty;
                if (GrdResult.SelectedItem is TrafficInfo)
                {
                    var cctv = GrdResult.SelectedItem as TrafficInfo;
                    cctvPath = cctv.Cctvurl;
                    //CCTVSCREEN.Address = $"{cctvPath}";
                    //CCTVSCREEN.SourceProvider.MediaPlayer.Play(new Uri(cctvPath));
                    _mediaPlayer.Play(new Media(_libVLC, new Uri(cctvPath)));
                    MAPSCREEN.Address = $"https://google.co.kr/maps/@{cctv.Coordy},{cctv.Coordx},14z";
                }
                else if(GrdResult.SelectedItem is FaTraffic)
                {
                    var cctv = GrdResult.SelectedItem as FaTraffic;
                    cctvPath = cctv.Cctvurl;
                    //CCTVSCREEN.Address = $"{cctvPath}";
                    //CCTVSCREEN.SourceProvider.MediaPlayer.Play(new Uri(cctvPath));
                    _mediaPlayer.Play(new Media(_libVLC, new Uri(cctvPath)));
                    MAPSCREEN.Address = $"https://google.co.kr/maps/@{cctv.Coordy},{cctv.Coordx},14z";
                }
                Debug.WriteLine(cctvPath);
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"CCTV로드 오류발생");
            }
        }

        private async void BtnFavorite_Click(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            TxtFindInfo.Text = string.Empty;

            List<FaTraffic> list = new List<FaTraffic>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if(conn.State == ConnectionState.Closed) conn.Open();

                    var query = @"SELECT Datacount,
                                         Roadsectionid,
                                         Filecreatetime,
                                         Cctvtype,
                                         Cctvurl,
                                         Cctvresolution,
                                         Coordx,
                                         Coordy,
                                         Cctvformat,
                                         Cctvname
                                    FROM fatraffic
                                   ORDER BY Cctvname ASC";
                    var cmd = new MySqlCommand(query, conn);
                    var adapter = new MySqlDataAdapter(cmd);
                    var dSet = new DataSet();
                    adapter.Fill(dSet, "FaTraffic");

                    foreach (DataRow dr in dSet.Tables["FaTraffic"].Rows)
                    {
                        list.Add(new FaTraffic
                        {
                            Datacount = Convert.ToInt32(dr["Datacount"]),
                            Roadsectionid = Convert.ToString(dr["Roadsectionid"]),
                            Filecreatetime = Convert.ToString(dr["Filecreatetime"]),
                            Cctvtype = Convert.ToString(dr["Cctvtype"]),
                            Cctvurl = Convert.ToString(dr["Cctvurl"]),
                            Cctvresolution = Convert.ToString(dr["Cctvresolution"]),
                            Coordx = Convert.ToString(dr["Coordx"]),
                            Coordy = Convert.ToString(dr["Coordy"]),
                            Cctvformat = Convert.ToString(dr["Cctvformat"]),
                            Cctvname = Convert.ToString(dr["Cctvname"])
                        });
                    }
                    this.DataContext = list;
                    StsResult.Content = $"즐겨찾기 {list.Count}건 조회완료";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB조회 오류 {ex.Message}");
            }
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 데이터를 선택하세요.(복수선택 가능)");
                return;
            }

            if (GrdResult.SelectedItem is FaTraffic)
            {
                await Commons.ShowMessageAsync("오류", "이미 즐겨찾기한 데이터입니다.");
                return;
            }

            try
            {
                // DB 
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO fatraffic
                                             (Datacount,
                                              Roadsectionid,
                                              Filecreatetime,
                                              Cctvtype,
                                              Cctvurl,
                                              Cctvresolution,
                                              Coordx,
                                              Coordy,
                                              Cctvformat,
                                              Cctvname)
                                       VALUES
                                             (@Datacount,
                                              @Roadsectionid,
                                              @Filecreatetime,
                                              @Cctvtype,
                                              @Cctvurl,
                                              @Cctvresolution,
                                              @Coordx,
                                              @Coordy,
                                              @Cctvformat,
                                              @Cctvname)";

                    var insRes = 0;
                    foreach (TrafficInfo item in GrdResult.SelectedItems)
                    {
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@Datacount", item.Datacount);
                            cmd.Parameters.AddWithValue("@Roadsectionid", item.Roadsectionid);
                            cmd.Parameters.AddWithValue("@Filecreatetime", item.Filecreatetime);
                            cmd.Parameters.AddWithValue("@Cctvtype", item.Cctvtype);
                            cmd.Parameters.AddWithValue("@Cctvurl", item.Cctvurl);
                            cmd.Parameters.AddWithValue("@Cctvresolution", item.Cctvresolution);
                            cmd.Parameters.AddWithValue("@Coordx", item.Coordx);
                            cmd.Parameters.AddWithValue("@Coordy", item.Coordy);
                            cmd.Parameters.AddWithValue("@Cctvformat", item.Cctvformat);
                            cmd.Parameters.AddWithValue("@Cctvname", item.Cctvname);

                            insRes += cmd.ExecuteNonQuery();

                    }
                    await Commons.ShowMessageAsync("저장", "DB저장 성공!");
                    StsResult.Content = $"DB저장 {insRes}건 성공";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB저장 오류 | {ex.Message}");
            }
        }

        private async void BtnDel_Click(object sender, RoutedEventArgs e)
        {
            if (!(GrdResult.SelectedItem is FaTraffic))
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기만 삭제할 수 있습니다.");
                return;
            }
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "삭제할 데이터를 선택하세요");
                return;
            }
            try         
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = "DELETE FROM fatraffic WHERE Cctvname = @Cctvname";
                    var delRes = 0;

                    foreach (FaTraffic item in GrdResult.SelectedItems)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Cctvname", item.Cctvname);

                        delRes += cmd.ExecuteNonQuery();
                    }
                    if (delRes == GrdResult.SelectedItems.Count)
                    {
                        await Commons.ShowMessageAsync("삭제", "DB삭제 성공!");
                        BtnFavorite_Click(sender, e);       //즐겨찾기 보기 이벤트 핸들러 한 번 실행
                        StsResult.Content = $"즐겨찾기 {delRes}건 삭제완료";     //  바로 즐겨찾기 조회창이 뜨기 때문에 화면에서 볼 수 없다
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("삭제", "DB삭제 일부 성공"); //발생 가능성 낮음
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB삭제오류 {ex.Message}");
            }
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            if(Dnum == 0)
            {
                Reload();
            }
            else if(Dnum == 1)
            {
                Reload2();
            }
        }

        private async void Reload()
        {
            string apiKey = "6f76c273dc584bcfbdcebe4edb606e6d";
            string openApiUri = $@"https://openapi.its.go.kr:9443/cctvInfo?apiKey={apiKey}" +
                                $"&type=ex&cctvType=1&minX=127.100000&maxX=128.890000&minY=34.100000&maxY=39.100000&getType=json";
            string result = string.Empty;

            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            var jsonResult = JObject.Parse(result);
            var data = jsonResult["response"]["data"];
            var json_array = data as JArray;
            var trafficInfo = new List<TrafficInfo>();
            foreach (var val in json_array)
            {
                var TrafficInfo = new TrafficInfo()
                {
                    Datacount = Convert.ToInt32(val["datacount"]),
                    Roadsectionid = Convert.ToString(val["roadsectionid"]),
                    Filecreatetime = Convert.ToString(val["filecreatetime"]),
                    Cctvtype = Convert.ToString(val["cctvtype"]),
                    Cctvurl = Convert.ToString(val["cctvurl"]),
                    Cctvresolution = Convert.ToString(val["cctvresolution"]),
                    Coordx = Convert.ToString(val["coordx"]),
                    Coordy = Convert.ToString(val["coordy"]),
                    Cctvformat = Convert.ToString(val["cctvformat"]),
                    Cctvname = Convert.ToString(val["cctvname"]),
                };
                trafficInfo.Add(TrafficInfo);
            }
            this.DataContext = trafficInfo;

            // 데이터 삭제
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = "DELETE FROM TrafficInfo";
                    var delRes = 0;

                    foreach (TrafficInfo item in GrdResult.Items)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            StsResult.Content = $"OpenAPI {trafficInfo.Count}건 삭제";
            
            // 데이터 추가
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO trafficinfo
                                             (Id,
                                              Datacount,
                                              Roadsectionid,
                                              Filecreatetime,
                                              Cctvtype,
                                              Cctvurl,
                                              Cctvresolution,
                                              Coordx,
                                              Coordy,
                                              Cctvformat,
                                              Cctvname)
                                       VALUES
                                             (@Id,
                                              @Datacount,
                                              @Roadsectionid,
                                              @Filecreatetime,
                                              @Cctvtype,
                                              @Cctvurl,
                                              @Cctvresolution,
                                              @Coordx,
                                              @Coordy,
                                              @Cctvformat,
                                              @Cctvname)";

                    var insRes = 0;
                    foreach (var temp in GrdResult.Items)
                    {
                        if (temp is TrafficInfo)
                        {
                            var item = temp as TrafficInfo;
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@Datacount", item.Datacount);
                            cmd.Parameters.AddWithValue("@Roadsectionid", item.Roadsectionid);
                            cmd.Parameters.AddWithValue("@Filecreatetime", item.Filecreatetime);
                            cmd.Parameters.AddWithValue("@Cctvtype", item.Cctvtype);
                            cmd.Parameters.AddWithValue("@Cctvurl", item.Cctvurl);
                            cmd.Parameters.AddWithValue("@Cctvresolution", item.Cctvresolution);
                            cmd.Parameters.AddWithValue("@Coordx", item.Coordx);
                            cmd.Parameters.AddWithValue("@Coordy", item.Coordy);
                            cmd.Parameters.AddWithValue("@Cctvformat", item.Cctvformat);
                            cmd.Parameters.AddWithValue("@Cctvname", item.Cctvname);

                            insRes += cmd.ExecuteNonQuery();
                        }
                    }
                    await Commons.ShowMessageAsync("로드", "고속도로 DB로드 성공!");
                    StsResult.Content = $"고속도로 DB {insRes}건 로드";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB로드 오류 | {ex.Message}");
            }
        }
        private async void Reload2()
        {
            string apiKey = "6f76c273dc584bcfbdcebe4edb606e6d";
            string openApiUri = $@"https://openapi.its.go.kr:9443/cctvInfo?apiKey={apiKey}" +
                                $"&type=its&cctvType=1&minX=127.100000&maxX=128.890000&minY=34.100000&maxY=39.100000&getType=json";
            string result = string.Empty;

            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            try
            {
                req = WebRequest.Create(openApiUri);
                res = await req.GetResponseAsync();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();

                Debug.WriteLine(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            var jsonResult = JObject.Parse(result);
            var data = jsonResult["response"]["data"];
            var json_array = data as JArray;
            var trafficInfo = new List<TrafficInfo>();
            foreach (var val in json_array)
            {
                var TrafficInfo = new TrafficInfo()
                {
                    Datacount = Convert.ToInt32(val["datacount"]),
                    Roadsectionid = Convert.ToString(val["roadsectionid"]),
                    Filecreatetime = Convert.ToString(val["filecreatetime"]),
                    Cctvtype = Convert.ToString(val["cctvtype"]),
                    Cctvurl = Convert.ToString(val["cctvurl"]),
                    Cctvresolution = Convert.ToString(val["cctvresolution"]),
                    Coordx = Convert.ToString(val["coordx"]),
                    Coordy = Convert.ToString(val["coordy"]),
                    Cctvformat = Convert.ToString(val["cctvformat"]),
                    Cctvname = Convert.ToString(val["cctvname"]),
                };
                trafficInfo.Add(TrafficInfo);
            }
            this.DataContext = trafficInfo;

            // 데이터 삭제
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = "DELETE FROM TrafficInfo";
                    var delRes = 0;

                    foreach (TrafficInfo item in GrdResult.Items)
                    {
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        delRes += cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            StsResult.Content = $"OpenAPI {trafficInfo.Count}건 삭제";

            // 데이터 추가
            try
            {
                using (MySqlConnection conn = new MySqlConnection(Commons.myConnString))
                {
                    if (conn.State == System.Data.ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO trafficinfo
                                             (Id,
                                              Datacount,
                                              Roadsectionid,
                                              Filecreatetime,
                                              Cctvtype,
                                              Cctvurl,
                                              Cctvresolution,
                                              Coordx,
                                              Coordy,
                                              Cctvformat,
                                              Cctvname)
                                       VALUES
                                             (@Id,
                                              @Datacount,
                                              @Roadsectionid,
                                              @Filecreatetime,
                                              @Cctvtype,
                                              @Cctvurl,
                                              @Cctvresolution,
                                              @Coordx,
                                              @Coordy,
                                              @Cctvformat,
                                              @Cctvname)";

                    var insRes = 0;
                    foreach (var temp in GrdResult.Items)
                    {
                        if (temp is TrafficInfo)
                        {
                            var item = temp as TrafficInfo;
                            MySqlCommand cmd = new MySqlCommand(query, conn);
                            cmd.Parameters.AddWithValue("@Datacount", item.Datacount);
                            cmd.Parameters.AddWithValue("@Roadsectionid", item.Roadsectionid);
                            cmd.Parameters.AddWithValue("@Filecreatetime", item.Filecreatetime);
                            cmd.Parameters.AddWithValue("@Cctvtype", item.Cctvtype);
                            cmd.Parameters.AddWithValue("@Cctvurl", item.Cctvurl);
                            cmd.Parameters.AddWithValue("@Cctvresolution", item.Cctvresolution);
                            cmd.Parameters.AddWithValue("@Coordx", item.Coordx);
                            cmd.Parameters.AddWithValue("@Coordy", item.Coordy);
                            cmd.Parameters.AddWithValue("@Cctvformat", item.Cctvformat);
                            cmd.Parameters.AddWithValue("@Cctvname", item.Cctvname);

                            insRes += cmd.ExecuteNonQuery();
                        }
                    }
                    await Commons.ShowMessageAsync("로드", "국도 DB로드 성공!");
                    StsResult.Content = $"국도 DB {insRes}건 로드";
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB로드 오류 | {ex.Message}");
            }
        }

        int num = 0;
        int Dnum;
        private void BtnSelectRoad_Click(object sender, RoutedEventArgs e)
        {
            num += 1;
            
            Dnum = num % 2;
            if(Dnum == 0)
            {
                StsResult.Content = $"고속도로 조회중";
                Reload();
            }
            else if(Dnum == 1)
            {
                StsResult.Content = $"국도 조회중";
                Reload2();
            }
        }

    } 
}