using ControlzEx.Standard;
using MahApps.Metro.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using wp11_MovieFinder.Logics;
using wp11_MovieFinder.Models;

namespace wp11_MovieFinder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool isFavorite = false;    // false -> 
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnNaverMovie_Click(object sender, RoutedEventArgs e)
        {
            await Commons.ShowMessageAsync("네이버영화", "네이버영화 사이트로 갑니다.");
        }

        // 검색버튼, 네이버API 영화 검색
        private async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtMovieName.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 영화명을 입력하세요.");
                return;
            }
            //if (TxtMovieName.Text.Length < 2)
            //{
            //    await Commons.ShowMessageAsync("검색", "검색어를 두글자 이상 입력하세요.");
            //    return;
            //}

            try
            {
                SearchMovie(TxtMovieName.Text);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"오류발생 : {ex.Message}");
            }
        }

        // 텍스트박스에서 키를 입력할때 엔터를 누르면 검색 시작
        private void TxtMovieName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchMovie_Click(sender, e);
            }
        }
        // 실제 검색 메서드
        private async void SearchMovie(string movieName)
        {
            string tmdb_apiKey = "fd2b7822a5956759efbe476287b3a0c0";
            string encording_movieName = HttpUtility.UrlDecode(movieName, Encoding.UTF8);
            string openApiUri = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}" +
                                $"&language=ko-KR&page=1&include_adult=false&query={encording_movieName}";
            string result = string.Empty;       //결과값

            //api 실행할 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            // TMDB API 요청
            try
            {
                req = WebRequest.Create(openApiUri);        // Url을 넣어서 객체를 생성
                res = await req.GetResponseAsync();    // 요청한 결과를 비동기 응답에 할당 .. 비동기로 동작하는 버튼
                reader = new StreamReader(res.GetResponseStream()); 
                result = reader.ReadToEnd();        // json 결과 텍스트로 저장

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

            //result를 json으로 변경
            var jsonResult = JObject.Parse(result); // string -> json

            var total = Convert.ToInt32(jsonResult["total_results"]);   //전체 검색결과 수
            //await Commons.ShowMessageAsync("검색결과", total.ToString());

            var items = jsonResult["results"];
            // items를 데이터그리드에 표시
            var json_array = items as JArray;

            var movieItems = new List<MovieItem>();     // json에서 넘어온 배열을 담을 장소
            foreach (var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Adult = Convert.ToBoolean(val["adult"]),
                    Id = Convert.ToInt32(val["id"]),
                    Original_Language = Convert.ToString(val["original_language"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Overview = Convert.ToString(val["overview"]),
                    Popularity = Convert.ToDouble(val["popularity"]),
                    Poster_Path = Convert.ToString(val["poster_path"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Title = Convert.ToString(val["title"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"])
                };
                movieItems.Add(MovieItem);
            }
            this.DataContext = movieItems;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtMovieName.Focus();   // 텍스트 박스에 포커스 set
        }

        // 그리드에서 셀을 선택하면 발생하는 이벤트 핸들러
        private void GrdResult_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                var movie = GrdResult.SelectedItem as MovieItem;
                Debug.WriteLine(movie.Poster_Path);
                if(string.IsNullOrEmpty(movie.Poster_Path)) // 포스터 이미지가 없다면 => No_Picture 나오게
                {
                    ImgPoster.Source = new BitmapImage(new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute));
                }
                else        // 포스터 이미지 경로가 존재한다면
                {
                    var base_url = "https://www.themoviedb.org/t/p/w600_and_h900_bestv2";
                    ImgPoster.Source = new BitmapImage(new Uri($"{base_url}{movie.Poster_Path}", UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                ImgPoster.Source = new BitmapImage(new Uri("/No_Picture.png", UriKind.RelativeOrAbsolute));
            }
        }

        // 영화 예고편 유튜브에서 보기
        private async void BtnWatchTrailer_Click(object sender, RoutedEventArgs e)
        {
            if(GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("유튜브", "영화를 선택하세요.");
                return;
            }

            if(GrdResult.SelectedItems.Count > 1) 
            {
                await Commons.ShowMessageAsync("유튜브", "영화를 하나만 선택하세요.");
                return;
            }

            string movieName = string.Empty;
            // 전체 넘기는 코드 가져오기

            var movie = GrdResult.SelectedItem as MovieItem;
            movieName = movie.Title;
            //movieName = (GrdResult.SelectedItem as MovieItem).Title;
            //await Commons.ShowMessageAsync("유튜브", $"예고편 볼 영화 {movieName}");
            var trailerWindow = new TrailerWindow(movieName);
            trailerWindow.Owner = this;     //TrailerWindow의 부모는 MainWindow
            trailerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;    // 부모창의 정중앙에 위치
            //trailerWindow.Show();       // 모달리스로 창을 열면 부모창 접근 가능 
            trailerWindow.ShowDialog();   // 모달창
        }

        //즐겨찾기 추가
        private async void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if(GrdResult.SelectedItems.Count ==0)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 영화를 선택하세요.(복수선택 가능)");
                return;
            }

            if(isFavorite )
            {
                await Commons.ShowMessageAsync("오류", "이미 즐겨찾기한 오류입니다.");
                return;
            }

            List<FavoriteMovieItem> list = new List<FavoriteMovieItem>();
            foreach (MovieItem item in GrdResult.SelectedItems)
            {
                var favoriteMovie = new FavoriteMovieItem
                {
                    Id = item.Id,
                    Title = item.Title,
                    Original_Title = item.Original_Title,
                    Original_Language = item.Original_Language,
                    Adult = item.Adult,
                    Overview = item.Overview,
                    Release_Date = item.Release_Date,
                    Vote_Average = item.Vote_Average,
                    Popularity = item.Popularity,
                    Poster_Path = item.Poster_Path,
                    Reg_Date = DateTime.Now     // 지금 저장하는 일시
                };
                list.Add(favoriteMovie);
            }

            try
            {
                // DB 
                using (SqlConnection conn = new SqlConnection(Commons.connString))
                {
                    if(conn.State == ConnectionState.Closed) conn.Open();
                    var query = @"INSERT INTO [dbo].[FavoriteMovieItem]
                                              ( [Id]
                                              , [Title]
                                              , [Original_Title]
                                              , [Release_Date]
                                              , [Original_Language]
                                              , [Adult]
                                              , [Popularity]
                                              , [Vote_Average]
                                              , [Poster_Path]
                                              , [Overview]
                                              , [Reg_Date] )
                                         VALUES
                                              ( @Id
                                              , @Title
                                              , @Original_Title
                                              , @Release_Date
                                              , @Original_Language
                                              , @Adult
                                              , @Popularity
                                              , @Vote_Average
                                              , @Poster_Path
                                              , @Overview
                                              , @Reg_Date )";

                    var insRes = 0;
                    foreach (FavoriteMovieItem item in list)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id",item.Id);
                        cmd.Parameters.AddWithValue("@Title", item.Title);
                        cmd.Parameters.AddWithValue("@Original_Title", item.Original_Title);
                        cmd.Parameters.AddWithValue("@Release_Date", item.Release_Date);
                        cmd.Parameters.AddWithValue("@Original_Language", item.Original_Language);
                        cmd.Parameters.AddWithValue("@Adult", item.Adult);
                        cmd.Parameters.AddWithValue("@Popularity", item.Popularity);
                        cmd.Parameters.AddWithValue("@Vote_Average", item.Vote_Average);
                        cmd.Parameters.AddWithValue("@Poster_Path", item.Poster_Path);
                        cmd.Parameters.AddWithValue("@Overview", item.Overview);
                        cmd.Parameters.AddWithValue("@Reg_Date", item.Reg_Date);

                        insRes += cmd.ExecuteNonQuery();
                    }
                    if(list.Count == insRes)
                    {
                        await Commons.ShowMessageAsync("저장", "즐겨찾기 추가 성공");
                    }
                    else
                    {
                        await Commons.ShowMessageAsync("저장", "즐겨찾기 추가 실패! 관리자에게 문의하세요.");
                    }
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류",$"DB저장 오류 {ex.Message}");
            }

        }
    }
}
