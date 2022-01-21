using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace MapEditor.Infrastructure
{
    public class NotifyObject : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName)
        {
            if (string.IsNullOrEmpty(propName))
            {
                return;
            }
            var handle = PropertyChanged;
            if (handle == null)
            {
                return;
            }
            handle(this, new PropertyChangedEventArgs(propName));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("propertyExpression is not a member");
            }
            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("propertyExpression is not a property");
            }
            MethodInfo getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
            {
                throw new ArgumentException("static property is not supported");
            }
            string propName = memberExpression.Member.Name;
            OnPropertyChanged(propName);
        }
    }
}
