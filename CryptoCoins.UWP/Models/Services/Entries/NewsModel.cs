using System;
using CryptoCoins.UWP.Helpers;

namespace CryptoCoins.UWP.Models.Services.Entries
{
    public class NewsModel : Observable
    {
        private DateTime _publicationTime;
        private Uri _link;
        private Uri _mediaLink;
        private string _source;
        private string _title;

        public string Source
        {
            get => _source;
            set => Set(ref _source, value);
        }

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        public Uri MediaLink
        {
            get => _mediaLink;
            set => Set(ref _mediaLink, value);
        }

        public DateTime PublicationTime
        {
            get => _publicationTime;
            set => Set(ref _publicationTime, value);
        }

        public Uri Link
        {
            get => _link;
            set => Set(ref _link, value);
        }
    }
}
