using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Windows;
using System.Windows.Controls;

using Demo.WindowsForms;
using GMap.NET;

namespace Demo.WindowsPresentation.Controls
{
   /// <summary>
   /// Interaction logic for TrolleyTooltip.xaml
   /// </summary>
   public partial class StopTooltip : UserControl
   {
   	  public event RoutedEventHandler RouteButtonClick;
   	
      public StopTooltip()
      {
         InitializeComponent();
      }

      public void SetValues(OSMStopPoint vl)
      {
  //       Operator.Text = vl.Operator;
         string type = "";
         labelMarshrutType.Text = vl.Name;
         LineNum.Text = vl.StopTypeName;
         AddButtons(vl, new OSMOTRouteTypes[] { OSMOTRouteTypes.Tramway }, null, "Трамваи");
         AddButtons(vl, new OSMOTRouteTypes[] { OSMOTRouteTypes.Trolleybus }, null, "Троллейбусы");
         AddButtons(vl, null, new OSMOTRouteTypes[] { OSMOTRouteTypes.Bus }, "Автобусы");
         AddButtons(vl, null, new OSMOTRouteTypes[] { OSMOTRouteTypes.SharedTaxi }, "Маршрутки");
         AddButtons(vl, new OSMOTRouteTypes[] { OSMOTRouteTypes.Train }, new OSMOTRouteTypes[] { OSMOTRouteTypes.Train | OSMOTRouteTypes.LongDistance }, "Поезда");
         AddButtons(vl, null, new OSMOTRouteTypes[] { OSMOTRouteTypes.Light_rail, OSMOTRouteTypes.Train | OSMOTRouteTypes.Local, OSMOTRouteTypes.Train | OSMOTRouteTypes.Regional }, "Электрички");
         AddButtons(vl, new OSMOTRouteTypes[] { OSMOTRouteTypes.Subway }, null, "Метро");

         //Number.Text = vl.Number;
//         TrackType.Text = vl.TrackType;
//         TimeGps.Text = vl.Time;
//         Area.Text = vl.AreaName;
//         Speed.Text = vl.Speed.ToString();
 //        Time.Text = vl.Time.ToString();
      }

      protected void AddButtons(OSMStopPoint vl, OSMOTRouteTypes[] generalTransTypes, OSMOTRouteTypes[] transTypes, string transTypeName)
      {
          IList<OSMOTRouteLite> tramRoutes1 = new List<OSMOTRouteLite>();
          Orientation orientation = Orientation.Horizontal;
          IEnumerable<OSMOTRouteLite> tramRoutes = null;
          IList<OSMOTRouteLite> numRefRoutes = new List<OSMOTRouteLite>();
          IList<OSMOTRouteLite> strRefRoutes = new List<OSMOTRouteLite>();
          if (generalTransTypes != null)
              foreach (OSMOTRouteTypes transType in generalTransTypes)
              {
                  if (((int)transType & (int)OSMOTRouteTypes.Train) > 0 || ((int)transType & (int)OSMOTRouteTypes.Light_rail) > 0)
                      orientation = Orientation.Vertical;
                  tramRoutes = from tramRoute in vl.Routes where ((int)tramRoute.RouteType == (int)transType) select tramRoute;
                  //      		tramRoutes1.Concat(tramRoutes);
                  if (tramRoutes.Count() > 0)
                  {
                      foreach (OSMOTRouteLite route in tramRoutes)
                      {
                          int refNumber = 0;
                          if (int.TryParse(route.Ref, out refNumber))
                              numRefRoutes.Add(route);
                          else
                              strRefRoutes.Add(route);
                      }
                  }
              }
          if(transTypes != null)
              foreach (OSMOTRouteTypes transType in transTypes)
              {
                  if (((int)transType & (int)OSMOTRouteTypes.Train) > 0 || ((int)transType & (int)OSMOTRouteTypes.Light_rail) > 0)
                      orientation = Orientation.Vertical;
                  tramRoutes = from tramRoute in vl.Routes where ((int)tramRoute.RouteType & (int)transType) == (int)transType select tramRoute;
                  if (tramRoutes.Count() > 0)
                  {
                      foreach (OSMOTRouteLite route in tramRoutes)
                      {
                          int refNumber = 0;
                          if (int.TryParse(route.Ref, out refNumber))
                              numRefRoutes.Add(route);
                          else
                              strRefRoutes.Add(route);
                      }
                  }
              }
          //      	tramRoutes.Clear();
          if (numRefRoutes.Count() > 0 || strRefRoutes.Count() > 0)
          {
              IOrderedEnumerable<OSMOTRouteLite> numRefRoutes1 = numRefRoutes.OrderBy(r => int.Parse(r.Ref));
              IOrderedEnumerable<OSMOTRouteLite> strRefRoutes1 = strRefRoutes.OrderBy(r => r.Ref);
              foreach (OSMOTRouteLite route in numRefRoutes1)
                  tramRoutes1.Add(route);
              foreach (OSMOTRouteLite route in strRefRoutes1)
                  tramRoutes1.Add(route);
              tramRoutes = tramRoutes1;
              panelRoutesRefs.Children.Add(new Label() { Content = transTypeName });
              WrapPanel panel = new WrapPanel();
              panel.Orientation = orientation;
              panel.Margin = new System.Windows.Thickness(6);
              foreach (OSMOTRouteLite route in tramRoutes)
              {
                  Button but = new Button() { Content = route.Ref, MinWidth = 20, MinHeight = 20 };
                  but.Tag = route;
                  but.Click += new RoutedEventHandler(OnRouteButtonClick);
                  panel.Children.Add(but);
              }
              panelRoutesRefs.Children.Add(panel);
          }
      }
      
      protected void OnRouteButtonClick(object sender, RoutedEventArgs e)
      {
      	if(RouteButtonClick != null)
      	{
      		RouteButtonClick(sender, e);
      	}
      }
   }
}
    