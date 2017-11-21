using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NP.XAMLIntellisenseExtensionForVS2017
{
    public class ResendEventBehavior
    {
        public static RoutedEvent CustomEvent = 
            EventManager.RegisterRoutedEvent
            (
                "Custom", 
                RoutingStrategy.Bubble, 
                typeof(RoutedEventHandler), 
                typeof(ResendEventBehavior));

        public RoutedEvent TheRoutedEvent { get; set; }


        public void Attach(FrameworkElement el)
        {
            el.AddHandler(TheRoutedEvent, (RoutedEventHandler)HandleEvent, true); 
        }

        private void HandleEvent(object sender, RoutedEventArgs e)
        {
            FrameworkElement el = (FrameworkElement)sender;

            RoutedEventArgs args = new RoutedEventArgs { RoutedEvent = CustomEvent };

            el.RaiseEvent(args);
        }

        public void Detach(FrameworkElement el)
        {
            el.RemoveHandler(TheRoutedEvent, (RoutedEventHandler)HandleEvent);
        }


        #region TheResendEventBehavior attached Property
        public static ResendEventBehavior GetTheResendEventBehavior(DependencyObject obj)
        {
            return (ResendEventBehavior)obj.GetValue(TheResendEventBehaviorProperty);
        }

        public static void SetTheResendEventBehavior(DependencyObject obj, ResendEventBehavior value)
        {
            obj.SetValue(TheResendEventBehaviorProperty, value);
        }

        public static readonly DependencyProperty TheResendEventBehaviorProperty =
        DependencyProperty.RegisterAttached
        (
            "TheResendEventBehavior",
            typeof(ResendEventBehavior),
            typeof(ResendEventBehavior),
            new PropertyMetadata(null, OnResendEventBehaviorChanged)
        );

        private static void OnResendEventBehaviorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement el = (FrameworkElement)d;

            ResendEventBehavior oldBehavior = e.OldValue as ResendEventBehavior;

            oldBehavior?.Detach(el);

            ResendEventBehavior newBehavior = e.NewValue as ResendEventBehavior;

            newBehavior?.Attach(el);
        }
        #endregion TheResendEventBehavior attached Property

    }
}
