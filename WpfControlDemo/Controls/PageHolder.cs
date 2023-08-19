using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace WpfControlDemo
{
    // https://codegrepr.com/question/page-datacontext-not-inherited-from-parent-frame/
    // https://stackoverflow.com/questions/3621424/page-datacontext-not-inherited-from-parent-frame
    public class PageHolder : Frame
    {
        public PageHolder()
            : base()
        {
            LoadCompleted += PageHolder_LoadCompleted;
            DataContextChanged += PageHolder_DataContextChanged;
        }

        private void PageHolder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateFrameDataContext();
        }

        private void PageHolder_LoadCompleted(object sender, NavigationEventArgs e)
        {
            UpdateFrameDataContext();
        }

        private void UpdateFrameDataContext()
        {
            if (Content is FrameworkElement content)
                content.DataContext = DataContext;
        }
    }
}
