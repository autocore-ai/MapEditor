using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor.WpfShell.Extensions
{
    internal class TextBoxExtensions : DependencyObject
    {
        public static readonly DependencyProperty MaxRowProperty = DependencyProperty.RegisterAttached(
            "MaxRow", 
            typeof(int), 
            typeof(TextBoxExtensions), 
            new PropertyMetadata(0));
        public static readonly DependencyProperty TextSourceProperty = DependencyProperty.RegisterAttached(
            "TextSource",
            typeof(ObservableCollection<string>),
            typeof(TextBoxExtensions), new PropertyMetadata(null, new PropertyChangedCallback(OnTextSourceChanged)));

        public static int GetMaxRow(DependencyObject obj) 
        {
            return (int)obj.GetValue(MaxRowProperty);
        }
        public static void SetMaxRow(DependencyObject obj, int value) 
        { 
            obj.SetValue(MaxRowProperty, value);
        }

        public static ObservableCollection<string> GetTextSource(DependencyObject obj) 
        { 
            return (ObservableCollection<string>)obj.GetValue(TextSourceProperty);
        }
        public static void SetTextSource(DependencyObject obj, ObservableCollection<string> value) 
        { 
            obj.SetValue(TextSourceProperty, value);
        }

        internal static void OnTextChanged(object sender, TextChangedEventArgs args) 
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
            {
                return;
            }
            //int maxRow = GetMaxRow(textBox);
            //if (maxRow > 0)
            //{
            //    int count = textBox.LineCount;
            //    if (count > maxRow) 
            //    {
            //        ObservableCollection<string> source = GetTextSource(textBox);
            //        if (source != null) 
            //        {
            //            for (int i = count - maxRow; i > 0; i--) 
            //            {
            //                source
            //            }
            //        }
            //    }
            //}
            textBox.ScrollToEnd();
        }

        public static void OnTextSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args) 
        {
            TextBox textBox = obj as TextBox;
            if (textBox == null)
            {
                return;
            }
            NotifyCollectionChangedEventHandler handler = (o, e) => 
            {
                int maxRow = GetMaxRow(textBox);
                ObservableCollection<string> source = GetTextSource(textBox);
                if (maxRow > 0 && source.Count > maxRow)
                {
                    textBox.Text = string.Join(Environment.NewLine, source.ToArray(), source.Count - maxRow, maxRow);
                }
                else
                {
                    textBox.Text = string.Join(Environment.NewLine, source);
                }
            };
            ObservableCollection<string> sourceOld = args.OldValue as ObservableCollection<string>;
            if (sourceOld != null) 
            { 
                sourceOld.CollectionChanged -= handler;
            }
            ObservableCollection<string> sourceNew = args.NewValue as ObservableCollection<string>;
            if (sourceNew != null) 
            {
                sourceNew.CollectionChanged += handler;
            }
            textBox.TextChanged += OnTextChanged;
        }
    }
}
