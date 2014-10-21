﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using XamlingCore.Portable.View.ViewModel;
using XamlingCore.XamarinThings.Content.Dynamic;
using XamlingCore.XamarinThings.Content.MasterDetail;

namespace XamlingCore.XamarinThings.Content.TabbedPages
{
    public class XTabbedPageView : TabbedPage
    {
        private XTabbedPageViewModel _viewModel;

        public XTabbedPageView()
        {
            ItemTemplate = new DataTemplate(typeof(DynamicContentPage));
        }

        protected override void OnBindingContextChanged()
        {
            _viewModel = BindingContext as XTabbedPageViewModel;

            if (_viewModel == null)
            {
                throw new ArgumentException("BindingContext must be XTabbedPageViewModel");
            }

            ItemsSource = _viewModel.SectionViewModels;

            base.OnBindingContextChanged();
        }

        protected override void OnCurrentPageChanged()
        {
            var page = CurrentPage;

            if (page == null)
            {
                return;
            }

            var vm = page.BindingContext as XViewModel;

            if (vm == null)
            {
                return;
            }

            Title = vm.Title;

            base.OnCurrentPageChanged();
        }
    }
}