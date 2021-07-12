using System;
using CryptoCoins.UWP.Models.StorageEntities;
using Telerik.Data.Core;

namespace CryptoCoins.UWP.Views.Entities
{
    public class FilterImp : IFilter
    {
        private readonly AlertFilterDescriptor _filterDescriptor;

        public FilterImp(AlertFilterDescriptor filterDescriptor)
        {
            _filterDescriptor = filterDescriptor;
        }

        public bool PassesFilter(object item)
        {
            var model = (AlertModel) item;
            return string.IsNullOrEmpty(_filterDescriptor.SearchQuery) ||
                   model.FromName.IndexOf(_filterDescriptor.SearchQuery, StringComparison.OrdinalIgnoreCase) != -1 ||
                   model.FromCode.IndexOf(_filterDescriptor.SearchQuery, StringComparison.OrdinalIgnoreCase) != -1;
        }
    }

    public class AlertFilterDescriptor : DelegateFilterDescriptor
    {
        private string _searchQuery;

        public AlertFilterDescriptor()
        {
            Filter = new FilterImp(this);
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
            }
        }
    }
}
