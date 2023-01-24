﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace StswExpress;

public abstract class StswContext : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public void NotifyPropertyChanged([CallerMemberName] string name = "none passed") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// SetProperty
    protected bool SetProperty<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;

        if (selectorExpression == null)
            throw new ArgumentNullException(nameof(selectorExpression));
        if (selectorExpression.Body is not MemberExpression body)
            throw new ArgumentException("The body must be a member expression");

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(body.Member.Name));
        return true;
    }
}
