using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.Web.Helpers
{
    public static class HtmlHelpers
    {
        #region Amazon
        public static MvcHtmlString AmazonLink(this HtmlHelper helper, string fileName, string directory = null)
        {
            var path = !string.IsNullOrEmpty(directory) ? directory + "/" + fileName : fileName;
            var url = string.Format(ApplicationCache.Instance.AmazonConfiguration.PathTemplate.Replace("{bucket}", ApplicationCache.Instance.AmazonConfiguration.BucketName));
            url = string.Concat(url, path).Replace("%20", "%2520");

            return MvcHtmlString.Create(url);
        }

        public static string AmazonLink(string fileName, string directory = null)
        {
            var path = !string.IsNullOrEmpty(directory) ? directory + "/" + fileName : fileName;
            var url = string.Format(ApplicationCache.Instance.AmazonConfiguration.PathTemplate.Replace("{bucket}", ApplicationCache.Instance.AmazonConfiguration.BucketName));

            return string.Concat(url, path).Replace("%20", "%2520");
        }
        #endregion

        #region AJAX Modals
        public static MvcHtmlString AjaxModalLink(this HtmlHelper helper, string text, string url, string title, string saveButton, string classes, string style = "", string deleteUrl = "", string id = "", bool insideModal = false, string modalSize = "", bool tooltip = true, string deleteBtnId = "")
        {
            var showTooltip = tooltip ? "title ='" + title + "'" : string.Empty;
            var insideModalHtml = insideModal ? " data-ajax='true' data-ajax-method='get' " : "role='button' data-toggle='modal' data-target='#ajax-modal' ";

            if (!string.IsNullOrEmpty(showTooltip))
            {
                return MvcHtmlString.Create($"<span data-toggle='tooltip' data-placement='top' {showTooltip}  > <a " + insideModalHtml + " " + (!string.IsNullOrEmpty(id) ? "id = '" + id + "'" : string.Empty) + " href = '" + url + "' class='ajax-modal " + classes + "' style='" + style + "' data-title='" + title + "' data-save-button='" + saveButton + "' data-delete-button='" + deleteUrl + "' data-delete-id='" + deleteBtnId + "' data-modal-size='" + modalSize + "'>" + text + "</a></span>");
            }

            return MvcHtmlString.Create("<a " + insideModalHtml + " " + (!string.IsNullOrEmpty(id) ? "id = '" + id + "'" : string.Empty) + " href = '" + url + "' class='ajax-modal " + classes + "' style='" + style + "' data-title='" + title + "' data-save-button='" + saveButton + "' data-delete-button='" + deleteUrl + "' data-delete-id='" + deleteBtnId + "' data-modal-size='" + modalSize + "'>" + text + "</a>");
        }

        public static MvcHtmlString AjaxModalLinkForSidebar(this HtmlHelper helper, string text, string url, string title, string saveButton, string classes, string style, string deleteUrl = "", string id = "", bool insideModal = false, string modalSize = "")
        {
            var insideModalHtml = insideModal ? " data-ajax='true' data-ajax-method='get' " : " role='button' data-toggle='modal' data-target='#ajax-modal' ";
            return MvcHtmlString.Create($"<a id='{id}' {insideModalHtml} href='{url}' class='ajax-modal {classes}' style='{style}' data-title='{title}' data-save-button='{saveButton}' data-delete-button='{deleteUrl}' data-modal-size='{modalSize}'>{text}</a>");
        }

        public static MvcHtmlString AjaxPlainModalLink(this HtmlHelper helper, string text, string url, string title, string saveButton, string classes, string style, string deleteUrl = "", string id = "", bool insideModal = false, int? modalSize = null)
        {
            var insideModalHtml = insideModal ? " data-ajax='true' data-ajax-method='get' " : " role='button' data-toggle='modal' data-target='#ajax-verification-modal' ";
            return MvcHtmlString.Create($"<a id='{id}' {insideModalHtml} href='{url}' class='ajax-modal-verification {classes}' style='{style}' data-title='{title}' data-modal-size='{modalSize}'>{text}</a>");
        }
        #endregion

        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> items, Func<T, string> text, Func<T, string> value = null, Func<T, Boolean> selected = null)
        {
            return items.Select(p => new SelectListItem
            {
                Text = text.Invoke(p),
                Value = value == null ? text.Invoke(p) : value.Invoke(p),
                Selected = selected?.Invoke(p) ?? false
            });
        }

        public static IEnumerable<SelectListItem> ToSelectList<T>(this IEnumerable<T> items, string firstItemText, Func<T, string> text, Func<T, string> value = null, Func<T, Boolean> selected = null)
        {
            var list = items.Select(p => new SelectListItem
            {
                Text = text.Invoke(p),
                Value = value == null ? text.Invoke(p) : value.Invoke(p),
                Selected = selected?.Invoke(p) == true
            });

            var newList = new List<SelectListItem>();

            if (firstItemText.IsNotNullOrEmpty())
            {
                newList.Add(new SelectListItem() { Value = string.Empty, Text = firstItemText });
            }

            newList.AddRange(list);

            return newList;
        }

        public static string IsActive(this HtmlHelper html, string control, string action, bool isButton = false)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            var returnActive = control.EqualsIgnoreCase(routeControl) && action.EqualsIgnoreCase(routeAction);

            if (isButton)
            {
                return returnActive ? "active" : string.Empty;
            }

            return returnActive ? "menu-item-active" : string.Empty;
        }

        public static string IsActiveSubChild(this HtmlHelper html, string control, string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = control.EqualsIgnoreCase(routeControl) && action.EqualsIgnoreCase(routeAction);

            return returnActive ? "menu-item-active" : string.Empty;
        }

        public static string HasActiveChild(this HtmlHelper html, string control, string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match

            foreach (var item in action.SplitToList())
            {
                var returnActive = control.EqualsIgnoreCase(routeControl) && item.EqualsIgnoreCase(routeAction);

                if (returnActive)
                {
                    return "menu-item-active menu-item-open";
                }
            }

            return string.Empty;
        }

        //public static MvcHtmlString GoogleMap(this HtmlHelper helper, GoogleMapVM map)
        //{
        //    var divId = !string.IsNullOrEmpty(map.DivId) ? map.DivId : Utilities.GenerateUniqueId();
        //    var markers = string.Join(",", map.Markers.Select(x => "{ id: '" + (!string.IsNullOrEmpty(x.Id) ? x.Id : Utilities.GenerateUniqueId()) + "', " + (x.HasCoordinates ? ("latitude: " + x.Latitude + ", longitude: " + x.Longitude) : "address: '" + (!string.IsNullOrEmpty(x.Address) ? x.Address.Replace("'", "").TrimStart('0') : "") + "'") + ", title: '" + (!string.IsNullOrEmpty(x.Title) ? x.Title.Replace("'", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Trim() : "") + "', icon: '" + x.Icon + "', html: '" + (!string.IsNullOrEmpty(x.Html) ? x.Html.Replace("'", "").Replace("\r\n", "").Replace("\r", "").Replace("\n", "").Trim() : "") + "', draggable: " + x.Draggable.ToString().ToLower() + "}"));

        //    var cluster = @"
        //        var markers = [];
        //  for (var i in $.goMap.markers) {
        //   var temp = $($.goMap.mapId).data($.goMap.markers[i]);
        //   markers.push(temp);
        //  }
        //  var markerclusterer = new MarkerClusterer($.goMap.map, markers, {imagePath: 'https://app.hughstonhomes.com/Assets/images/map-icons/m'});";

        //    var parcelPopups = @"
        //      // Cause click on map to show attribute popup and highlight parcel.

        //      var infoWindow = null;
        //      var mapFeatures = [];
        //      google.maps.event.addListener($.goMap.map, 'click', function(event) {
        //        if ($.goMap.map.getZoom() < REP.Layer.Google.MIN_ZOOM) return;

        //        // Close any previous InfoWindow and hide any previous features.
        //        if (infoWindow !== null) infoWindow.close();
        //        infoWindow = null;
        //        for (var i = 0; i < mapFeatures.length; i++) mapFeatures[i].setMap(null);
        //        mapFeatures = [];

        //        var latLng = event.latLng;

        //        REP.Layer.Google.IdentifyByPoint($.goMap.map, latLng, function(resp) {
        //          var wText = '<table class=""parcel-table""><tr><th>Parcel Information</th></tr>';
        //          if (resp.results.length) {
        //            var respRow0 = resp.results[0];
        //            for(var respKey in respRow0) {
        //                var respVal = respRow0[respKey];
        //                if (respVal === null) continue;
        //                if (respKey === 'geom') {
        //                    // Add parcel geometry (possibly multiple if multipart) to map.
        //                    for (var i = 0; i < respVal.length; i++) {
        //                        respVal[i].setOptions({fillColor: 'rgb(144,238,144)', strokeColor: 'rgb(200,0,0)'});
        //                        respVal[i].setMap($.goMap.map);
        //                        mapFeatures.push(respVal[i]);
        //                    }
        //                } else if (respKey === 'buildings_poly') {
        //                    // Iterate through each building record.
        //                    //   for (var bldgRecIdx = 0; bldgRecIdx < respVal.length; bldgRecIdx++) {
        //                    //     var bldgRec = respVal[bldgRecIdx];
        //                    //     if (typeof(bldgRec['geom']) === 'undefined' || bldgRec['geom'] === null) continue;
        //                    //    var bldgRecGeoms = bldgRec['geom'];
        //                        // Add each building geometry to map.
        //                    //     for (var i = 0; i < bldgRecGeoms.length; i++) {
        //                        //bldgRecGeoms[i].setOptions({strokeColor: 'rgb(255,128,255)', fillOpacity: 0.0, clickable: false});
        //                        //bldgRecGeoms[i].setMap(map);
        //                    //      mapFeatures.push(bldgRecGeoms[i]);
        //                    //    }
        //                    //  }
        //                } else {
        //                    if (respKey.endsWith('_id')){
        //                        continue;
        //                    }
        //                    //if (wText !== '') wText += '\n<br>';state_abbr
        //                    else if (respKey == 'county_name'){
        //                        wText += '<tr><td>county</td><td> ' + respVal + '</td></tr>';
        //                    }
        //                    else if (respKey == 'muni_name'){
        //                        wText += '<tr><td>city</td><td> ' + respVal + '</td></tr>';
        //                    }
        //                    else if (respKey == 'state_abbr'){
        //                        wText += '<tr><td>state</td><td> ' + respVal + '</td></tr>';
        //                    }
        //                    else if (respKey == 'census_zip'){
        //                        wText += '<tr><td>zip</td><td> ' + respVal + '</td></tr>';
        //                    }
        //                    else {
        //                        wText += '<tr><td>' +respKey + '</td><td> ' + respVal + '</td></tr>';
        //                    }
        //                }
        //            }
        //            wText += '</table>';
        //            infoWindow = new google.maps.InfoWindow({position: latLng, content: wText});
        //            infoWindow.open($.goMap.map);
        //          }
        //        }, function(errObj) {
        //          alert('REP Overlays error: ' + errObj.message);
        //        });
        //      });
        //      REP.Layer.Google.Initialize($.goMap.map, { 'Return_Buildings': true });";

        //    var kmlLayers = "";

        //    foreach (var kmlFile in map.KmlLayers)
        //    {
        //        var randomId = Utilities.GenerateUniqueId();

        //        kmlLayers += @"
        //            var kmlLayer" + randomId + @" = new google.maps.KmlLayer('" + kmlFile + @"', {
        //                suppressInfoWindows: true,
        //                preserveViewport: false,
        //                map: $.goMap.map
        //            });
        //        ";
        //    }

        //    var html = @"
        //        <div id='" + divId + "' style='" + map.Style + "' class='" + map.Classes + @"'></div>

        //        <!--parcel overlay script-->
        //        <script type='text/javascript' src='https://reportallusa.com/overlay/js.php?v=3&map=Google&client=AiGDTFQtH0'></script>

        //        <script type='text/javascript'>
        //            $(function () {
        //                $('#" + divId + @"').goMap({
        //                    maptype: '" + map.MapType + @"',
        //                    zoom: " + map.Zoom + @",
        //                    gestureHandling: 'none',
        //                    " + (map.AllowNewMarker ? "addMarker: 'single'," : "") + @"
        //                    disableDoubleClickZoom: " + map.DisableDoubleClickZoom.ToString().ToLower() + @",
        //                    markers: [" + markers + @"]
        //                });
        //                $.goMap.map.set('gestureHandling', 'cooperative');
        //                " + (map.FitAllMarkersInBounds ? "$.goMap.fitBounds('visible'); " : "") + @"
        //                " + (map.CreateClusters ? cluster : "") + @"
        //                " + (map.AdvancedParcelData ? parcelPopups : "") + @"
        //                " + kmlLayers + @"
        //            });
        //        </script>
        //    ";

        //    return MvcHtmlString.Create(html);
        //}

        public static MvcHtmlString Tooltip(this HtmlHelper helper, string tooltip, string text = "<i class='ti-help-alt'></i>")
        {
            return MvcHtmlString.Create("<a class='mytooltip tooltip-effect-9' href='#'>" + text + "<span class='tooltip-content3'>" + tooltip + "</span></a>");
        }

        //public static MvcHtmlString DatePickerFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string classes = null)
        //{
        //    var data = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
        //    string propertyName = data.PropertyName;
        //    string className = data.DataTypeName;
        //    return MvcHtmlString.Create(@"
        //        <input type=""text"" id=""" + className + "_" + propertyName + @""" name=""" + className + "." + propertyName + @""" value=""" + DateTime.Now.Date.ToShortDateString() + @""" + class=""" + classes + @""" />

        //        <script type=""text/javascript"">
        //            $(function() {
        //                $('input[name=""" + className + "." + propertyName + @"""]').daterangepicker({
        //                    singleDatePicker: true,
        //                    showDropdowns: true
        //                },
        //                function() {
        //                    //optional function to execute on change
        //                });
        //            });
        //        </script>
        //    ");
        //}

        public static MvcHtmlString DatePicker(this HtmlHelper helper, string inputName)
        {
            return MvcHtmlString.Create(@"
                <input type=""text"" name=""" + inputName + @""" value=""" + DateTime.Now.Date.ToShortDateString() + @""" />

                <script type=""text/javascript"">
                    $(function() {
                        $('input[name=""" + inputName + @"""]').daterangepicker({
                            singleDatePicker: true,
                            showDropdowns: true
                        },
                        function() {
                            //optional function to execute on change
                        });
                    });
                </script>
            ");
        }

        public static MvcHtmlString DateRangePicker(this HtmlHelper helper, string fromDateName, string toDateName, string classes = null)
        {
            var fromDateId = Utilities.GenerateUniqueId();
            var toDateId = Utilities.GenerateUniqueId();
            var id = Utilities.GenerateUniqueId();

            return MvcHtmlString.Create(
            @"<input type=""hidden"" name=""" + fromDateName + @""" id=""" + fromDateId + @""" />
            <input type=""hidden"" name=""" + toDateName + @""" id=""" + toDateId + @""" />
            <div id=""" + id + @""" class=""pull-right " + classes + @""" style=""background: #fff; cursor: pointer; padding: 5px 10px; border: 1px solid #ccc; width: 100%"">
                <i class=""glyphicon glyphicon-calendar fa fa-calendar""></i>&nbsp;
                <span></span> <b class=""caret""></b>
            </div>

            <script type=""text/javascript"">
                $(function () {

                    var start = moment().subtract(29, 'days');
                    var end = moment();

                    function cb(start, end) {
                        $('#" + id + @" span').html(start.format('MMMM D, YYYY') + ' - ' + end.format('MMMM D, YYYY'));

                        $('#" + fromDateId + @"').val(start.format('YYYY-MM-DD'));
                        $('#" + toDateId + @"').val(end.format('YYYY-MM-DD'));
                    }

                    $('#" + id + @"').daterangepicker({
                        startDate: start,
                        endDate: end,
                        ranges: {
                            'Today': [moment(), moment()],
                            'Yesterday': [moment().subtract(1, 'days'), moment().subtract(1, 'days')],
                            'Last 7 Days': [moment().subtract(6, 'days'), moment()],
                            'Last 30 Days': [moment().subtract(29, 'days'), moment()],
                            'This Month': [moment().startOf('month'), moment().endOf('month')],
                            'Last Month': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
                        }
                    }, cb);

                    cb(start, end);

                });
            </script>");
        }

        public static MvcHtmlString RenderModuleLink(this HtmlHelper htmlHelper, Modules module)
        {
            var accessLevel = Utilities.GetAccessLevel(module.Id, SessionVariables.AllPermissions);
            var canEdit = SessionVariables.CurrentUser.IsSuperAdmin || accessLevel == Operations.ReadWrite;

            if (canEdit)
            {
                return htmlHelper.AjaxModalLink(module.Name, "/settings/editmodule/" + module.Id, "Edit Module", "Save", string.Empty, string.Empty, "/settings/deletemodule/" + module.Id);
            }
            else
            {
                return new MvcHtmlString(htmlHelper.Encode(module.Name));
            }
        }

        public static string ToCompleteImagePath(this string fileName)
        {
            //var siteUrl = "SiteUrl".AppSetting<string>("https://app.praisecms.com");
            var siteUrl = ApplicationCache.Instance.EnvironmentConfiguration.Url;
            string imagesDirectoryUrl = "Upload.Images".AppSetting("/Uploads/Images");

            return $"{siteUrl}{imagesDirectoryUrl}/{fileName}";
        }

        public static string WidgetPartialViewUrl(this string value)
        {
            return value.IsNotNullOrEmpty() ? $"~/Views/DashboardTemplates/Widgets/{value}" : value;
        }

        //public static MvcHtmlString DisplayDataTable(this HtmlHelper helper, DataTable dataTable, string id, string classes, string style, IEnumerable<string> hiddenColumns = null)
        //{
        //    var html = @"
        //        <table id='{id}' class='{classes}'>
        //            <thead>
        //                <tr>
        //                    {headers}
        //                </tr>
        //            </thead>
        //            <tbody>
        //                {rows}
        //            </tbody>
        //        </table>
        //    ";

        //    hiddenColumns = hiddenColumns ?? new List<string>();

        //    var columnHeaders = dataTable.Columns.Cast<DataColumn>().Where(x => !hiddenColumns.Contains(x.ColumnName)).Select(x => "<th>" + x.ColumnName + "</th>").ToList();

        //    html = html.Replace("{id}", id);
        //    html = html.Replace("{classes}", classes);
        //    html = html.Replace("{headers}", string.Join("", columnHeaders));

        //    var columnNames = new List<string>();

        //    var rows = "";

        //    for (int i = 0; i < dataTable.Rows.Count; i++)
        //    {
        //        rows += "<tr>";

        //        for (int j = 0; j < dataTable.Columns.Count; j++)
        //        {
        //            if (!hiddenColumns.Contains(dataTable.Columns[j].ColumnName))
        //            {
        //                var value = dataTable.Rows[i][j].ToString();

        //                rows += string.Format("<td>{0}</td>", !string.IsNullOrWhiteSpace(value) ? value : "");
        //            }
        //        }

        //        rows += "</tr>";
        //    }

        //    html = html.Replace("{rows}", rows);

        //    return MvcHtmlString.Create(html);
        //}

        //public static Highcharts BuildGraph(this HtmlHelper helper, GraphVM graph, int? width = null, int? height = null)
        //{
        //    switch (graph.Type)
        //    {
        //        case GraphTypes.PieChart:
        //            {
        //                return loadPieChart(graph, width, height);
        //            }
        //        case GraphTypes.BarGraph:
        //            {
        //                return loadBarGraph(graph, width, height);
        //            }
        //        case GraphTypes.LineGraph:
        //            {
        //                return loadLineGraph(graph, width, height);
        //            }
        //        case GraphTypes.AreaGraph:
        //            {
        //                return loadAreaGraph(graph, width, height);
        //            }
        //    }

        //    return null;
        //}

        //#region Private Methods

        //private static Highcharts loadPieChart(GraphVM graph, int? width = null, int? height = null)
        //{
        //    if (graph.YAxisValues.Count == 1)
        //    {
        //        var values = graph.YAxisValues.ElementAt(0);

        //        var total = values.Sum();
        //        var data = new object[graph.DataTable.Rows.Count];

        //        for (var i = 0; i < graph.DataTable.Rows.Count; i++)
        //        {
        //            var name = graph.XAxisValues[i];
        //            var value = values[i];
        //            var percent = Math.Round(value / total, 2);

        //            data[i] = new object[] { name, percent };
        //        }

        //        var chart = new Highcharts("pie_graph_" + graph.Id)
        //            .InitChart(new Chart { PlotShadow = false, Width = width, Height = height })
        //            .SetTitle(new Title { Text = graph.Title })
        //            .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage.toFixed(2) +' %'; }" })
        //            .SetPlotOptions(new PlotOptions
        //            {
        //                Pie = new PlotOptionsPie
        //                {
        //                    AllowPointSelect = true,
        //                    Cursor = Cursors.Pointer,
        //                    DataLabels = new PlotOptionsPieDataLabels
        //                    {
        //                        Color = ColorTranslator.FromHtml("#000000"),
        //                        ConnectorColor = ColorTranslator.FromHtml("#000000"),
        //                        Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage.toFixed(2) +' %'; }"
        //                    }
        //                }
        //            })
        //            .SetSeries(new Series
        //            {
        //                Type = ChartTypes.Pie,
        //                Name = graph.XAxisTitle,
        //                Data = new DotNet.Highcharts.Helpers.Data(data)
        //            });

        //        return chart;
        //    }

        //    throw new Exception("Pie charts can and must have only a single series defined.");
        //}

        //private static Highcharts loadBarGraph(GraphVM graph, int? width = null, int? height = null)
        //{
        //    var series = new Series[graph.YAxisObjects.Count];
        //    var rotateXAxisLabel = graph.RotateXAxisLabels ? new XAxisLabels() { Rotation = 270 } : null;

        //    for (int i = 0; i < series.Length; i++)
        //    {
        //        series[i] = new Series { Name = graph.YAxisColumns.ElementAt(i), Data = new DotNet.Highcharts.Helpers.Data(graph.YAxisObjects.ElementAt(i).ToArray()) };
        //    }

        //    var chart = new Highcharts("bar_graph_" + graph.Id)
        //        .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column, Width = width, Height = height })
        //        .SetTitle(new Title { Text = graph.Title })
        //        .SetSubtitle(new Subtitle { Text = graph.SubTitle })
        //        .SetXAxis(new XAxis { Categories = graph.XAxisValues.ToArray(), Title = new XAxisTitle() { Text = graph.XAxisTitle }, Labels = rotateXAxisLabel })
        //        .SetYAxis(new YAxis
        //        {
        //            Min = 0,
        //            Title = new YAxisTitle { Text = graph.YAxisTitle }
        //        })
        //        .SetTooltip(new Tooltip { Formatter = @"function() { return ''+ this.x +': '+ this.y +' '; }" })
        //        .SetPlotOptions(new PlotOptions
        //        {
        //            Column = new PlotOptionsColumn
        //            {
        //                PointPadding = 0.2,
        //                BorderWidth = 0
        //            }
        //        })
        //        .SetLegend(new Legend
        //        {
        //            Layout = Layouts.Vertical,
        //            Align = HorizontalAligns.Right,
        //            VerticalAlign = VerticalAligns.Top,
        //            X = -10,
        //            Y = 100,
        //            BorderWidth = 0,
        //            Enabled = graph.ShowLegend
        //        })
        //        .SetSeries(series);

        //    return chart;
        //}

        //private static Highcharts loadLineGraph(GraphVM graph, int? width = null, int? height = null)
        //{
        //    var series = new Series[graph.YAxisObjects.Count];
        //    var rotateXAxisLabel = graph.RotateXAxisLabels ? new XAxisLabels() { Rotation = 270 } : null;
        //    var id = !string.IsNullOrEmpty(graph.Id) ? graph.Id : Utilities.GenerateUniqueId();

        //    for (int i = 0; i < series.Length; i++)
        //    {
        //        series[i] = new Series { Name = graph.YAxisColumns.ElementAt(i), Data = new DotNet.Highcharts.Helpers.Data(graph.YAxisObjects.ElementAt(i).ToArray()) };
        //    }

        //    var chart = new Highcharts("line_graph_" + id)
        //        .InitChart(new Chart
        //        {
        //            DefaultSeriesType = ChartTypes.Line,
        //            ClassName = "line_graph",
        //            Width = width,
        //            Height = height
        //        })
        //        .SetTitle(new Title
        //        {
        //            Text = graph.Title
        //        })
        //        .SetSubtitle(new Subtitle
        //        {
        //            Text = graph.SubTitle
        //        })
        //        .SetXAxis(new XAxis { Categories = graph.XAxisValues.ToArray(), Title = new XAxisTitle() { Text = graph.XAxisTitle }, Labels = rotateXAxisLabel })
        //        .SetYAxis(new YAxis
        //        {
        //            Title = new YAxisTitle { Text = graph.YAxisTitle },
        //            PlotLines = new[]
        //            {
        //                new YAxisPlotLines
        //                {
        //                    Value = 0,
        //                    Width = 1,
        //                    Color = ColorTranslator.FromHtml("#808080")
        //                }
        //            }
        //        })
        //        .SetTooltip(new Tooltip
        //        {
        //            Formatter = @"function() {
        //                                return '<b>'+ this.series.name +'</b><br/>'+
        //                            this.x +': '+ this.y +'';
        //                        }"
        //        })
        //        .SetLegend(new Legend
        //        {
        //            Layout = Layouts.Vertical,
        //            Align = HorizontalAligns.Right,
        //            VerticalAlign = VerticalAligns.Top,
        //            X = -10,
        //            Y = 100,
        //            BorderWidth = 0,
        //            Enabled = graph.ShowLegend
        //        })
        //        .SetSeries(series);

        //    return chart;
        //}

        //private static Highcharts loadAreaGraph(GraphVM graph, int? width = null, int? height = null)
        //{
        //    var series = new Series[graph.YAxisObjects.Count];
        //    var rotateXAxisLabel = graph.RotateXAxisLabels ? new XAxisLabels() { Rotation = 270 } : null;
        //    var id = !string.IsNullOrEmpty(graph.Id) ? graph.Id : Utilities.GenerateUniqueId();

        //    for (int i = 0; i < series.Length; i++)
        //    {
        //        series[i] = new Series { Name = graph.YAxisColumns.ElementAt(i), Data = new DotNet.Highcharts.Helpers.Data(graph.YAxisObjects.ElementAt(i).ToArray()) };
        //    }

        //    var chart = new Highcharts("area_graph_" + id)
        //        .InitChart(new Chart
        //        {
        //            DefaultSeriesType = ChartTypes.Area,
        //            ClassName = "area_graph",
        //            Width = width,
        //            Height = height
        //        })
        //        .SetTitle(new Title
        //        {
        //            Text = graph.Title
        //        })
        //        .SetSubtitle(new Subtitle
        //        {
        //            Text = graph.SubTitle
        //        })
        //        .SetXAxis(new XAxis { Categories = graph.XAxisValues.ToArray(), Title = new XAxisTitle() { Text = graph.XAxisTitle }, Labels = rotateXAxisLabel })
        //        .SetYAxis(new YAxis
        //        {
        //            Title = new YAxisTitle { Text = graph.YAxisTitle },
        //            PlotLines = new[]
        //            {
        //                new YAxisPlotLines
        //                {
        //                    Value = 0,
        //                    Width = 1,
        //                    Color = ColorTranslator.FromHtml("#808080")
        //                }
        //            }
        //        })
        //        .SetTooltip(new Tooltip
        //        {
        //            Formatter = @"function() {
        //                                return '<b>'+ this.series.name +'</b><br/>'+
        //                            this.x +': '+ this.y +'';
        //                        }"
        //        })
        //        .SetLegend(new Legend
        //        {
        //            Layout = Layouts.Vertical,
        //            Align = HorizontalAligns.Right,
        //            VerticalAlign = VerticalAligns.Top,
        //            X = -10,
        //            Y = 100,
        //            BorderWidth = 0,
        //            Enabled = graph.ShowLegend
        //        })
        //        .SetSeries(series);

        //    return chart;
        //}
        //#endregion
    }
}