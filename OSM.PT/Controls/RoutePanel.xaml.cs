/*
 * Сделано в SharpDevelop.
 * Пользователь: Родион
 * Дата: 08.03.2013
 * Время: 1:01
 * 
 * Для изменения этого шаблона используйте Сервис | Настройка | Кодирование | Правка стандартных заголовков.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Demo.WindowsForms;

namespace Demo.WindowsPresentation.Controls
{
	/// <summary>
	/// Interaction logic for RoutePanel.xaml
	/// </summary>
	public partial class RoutePanel : UserControl
	{
		public RoutePanel()
		{
			InitializeComponent();
		}
		
		public void SetValues(OSMOTRoute route)
		{
			string strRouteType = "";
			switch(route.RouteType)
			{
				case OSMOTRouteTypes.Bus : 
					{
						strRouteType = "Автобус ";
						break;
					}
				case OSMOTRouteTypes.SharedTaxi : 
					{
						strRouteType = "Маршрутное такси ";
						break;
					}
				case OSMOTRouteTypes.Tramway : 
					{
						strRouteType = "Трамвай ";
						break;
					}
				case OSMOTRouteTypes.Trolleybus : 
					{
						strRouteType = "Троллейбус ";
						break;
					}
				case OSMOTRouteTypes.Train : 
					{
						strRouteType = "Поезд ";
						break;
					}
			}
			lblRef.Text = String.Format("{0} {1}", strRouteType, route.Ref);
			lblName.Text = route.Name;
//			lblOperator.Text = route.
			foreach(OSMStopPoint stop in route.Stops)
			{
				Button but = new Button();
				but.Content = stop.Name;
				stkForwardStops.Children.Add(but);
			}
		}
	}
}