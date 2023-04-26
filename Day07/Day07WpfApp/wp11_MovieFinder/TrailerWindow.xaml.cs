﻿using CefSharp.DevTools.WebAudio;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using wp11_MovieFinder.Models;

namespace wp11_MovieFinder
{
    /// <summary>
    /// TrailerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TrailerWindow : MetroWindow
    {
        List<YoutubeItem> youtubeItems = null;      //검색결과 담을 리스트
        public TrailerWindow()
        {
            InitializeComponent();
        }

        // 부모에서 데이터를 가져오려면 재정의 생성자 만들어야함
        public TrailerWindow(string movieName) : this()
        {
            LblMovieName.Content = $"{movieName} 예고편";
        }

        // 부모에서 객체를 통채로 가져오기 가능
        public TrailerWindow(MovieItem movie) : this()
        {
            LblMovieName.Content = $"{movie.Title} 예고편";
        }

        //화면 로드 완료후에 YouTubeAPI 실행
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            youtubeItems = new List<YoutubeItem>();     //초기화
            SearchYoutubeApi();
        }

        private async void SearchYoutubeApi()
        {
            await LoadDataCollection();
            LsvResult.ItemsSource = youtubeItems;
        }

        private async Task LoadDataCollection()
        {
            var youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = "AIzaSyBFgCk2MCu91cEjgZJdu8eqQjNYjJKCx8c",
                    ApplicationName = this.GetType().ToString()
                }
                );
            var req = youtubeService.Search.List("snippet");
            req.Q = LblMovieName.Content.ToString();
            req.MaxResults = 10;

            var res = await req.ExecuteAsync();         //검색결과 가져옴

            Debug.WriteLine("-------------------유튜브 검색결과-------------------");
            foreach ( var item in res.Items )
            {
                Debug.WriteLine(item.Snippet.Title);
                if(item.Id.Kind.Equals("youtube#video"))
                {
                    YoutubeItem youtube = new YoutubeItem()
                    {
                        Title = item.Snippet.Title,
                        ChannelTitle = item.Snippet.ChannelTitle,
                        URL = $"https://www.youtube.com/watch?v={item.Id.VideoId}",
                        //Author = item.Snippet.ChannelTitle
                    };
                    youtube.Thumbnail = new BitmapImage(new Uri(item.Snippet.Thumbnails.Default__.Url,
                                                                    UriKind.RelativeOrAbsolute));
                    youtubeItems.Add(youtube);  
                }
            }
        }

        private void LsvResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(LsvResult.SelectedItem is YoutubeItem)
            {
                var video = LsvResult.SelectedItem as YoutubeItem;
                BrsYoutube.Address = video.URL;
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BrsYoutube.Address = string.Empty;  //웹브라우저 주소 클리어
            BrsYoutube.Dispose();       // 리소스 해제(!)
        }
    }
}
