using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace StswExpress
{
    /// <summary>
    /// Basic class for DataContext of Windows and UserControls to property change notifying
    /// </summary>
    public abstract class D : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string name = "none passed") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            //if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;

            if (selectorExpression == null)
                throw new ArgumentNullException("selectorExpression");
            if (!(selectorExpression.Body is MemberExpression body))
                throw new ArgumentException("The body must be a member expression");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(body.Member.Name));
            return true;
        }
    }
}
