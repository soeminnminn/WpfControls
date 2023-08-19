using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ObjectTree.Models;

namespace ObjectTree
{
    public class ItemTemplateSelector : DataTemplateSelector
    {
        #region Variables
        private static Dictionary<string, DataTemplate> dataTemplateCache = new Dictionary<string, DataTemplate>();
        private static ResourceDictionary _localDictionary = null;
        #endregion

        #region Properties
        private static ResourceDictionary LocalDictionary
        {
            get
            {
                if (_localDictionary == null)
                {
                    _localDictionary = new ResourceDictionary()
                    {
                        Source = new System.Uri("pack://application:,,,/ObjectTree;component/Themes/Generic.xaml")
                    };
                }
                return _localDictionary;
            }
        }
        #endregion

        #region Methods
        private bool TryGetValue(string key, FrameworkElement fe, out DataTemplate template)
        {
            if (dataTemplateCache.TryGetValue(key, out template))
                return true;

            template = fe.TryFindResource(key) as DataTemplate;
            if (template != null)
            {
                dataTemplateCache.Add(key, template);
                return true;
            }
            else
            {
                var localDict = LocalDictionary;
                if (localDict.Contains(key))
                {
                    template = localDict[key] as DataTemplate;
                    if (template != null)
                    {
                        dataTemplateCache.Add(key, template);
                        return true;
                    }
                }
            }

            return false;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement cr && item is MemberNode node)
            {
                DataTemplate temp;
                if (node.IsObjectRef && TryGetValue("OtTemplateObjectRef", cr, out temp))
                    return temp;

                if (node.IsNull && TryGetValue("OtTemplateNull", cr, out temp))
                    return temp;

                if (node.IsPrimitive && TryGetValue("OtTemplatePrimitive", cr, out temp))
                    return temp;

                if (node.IsString && TryGetValue("OtTemplateString", cr, out temp))
                    return temp;

                if (node.IsEnum && TryGetValue("OtTemplateEnum", cr, out temp))
                    return temp;

                if (node.IsBlob && TryGetValue("OtTemplateBlob", cr, out temp))
                    return temp;

                if (node.IsNotNullArray && TryGetValue("OtTemplateArray", cr, out temp))
                    return temp;

                if (node.IsObject && TryGetValue("OtTemplateObject", cr, out temp))
                    return temp;
            }

            return base.SelectTemplate(item, container);
        }
        #endregion
    }
}
