﻿using System.Collections.Generic;
namespace OnlineChat.Models.News
{
    public interface INewsRepository
    {
        public IEnumerable<NewsModel> GetAllNews();

        public IEnumerable<NewsModel> GetAllNewsBetween(int fromId, int toId);

        public IEnumerable<NewsModel> GetLastTenNews();

        public bool AddNews(NewsModel news);

        public bool DeleteNews(int id);
    }
}
