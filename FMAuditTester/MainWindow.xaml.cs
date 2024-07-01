using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using DnsClient;

namespace FMAuditTester
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public class URL : INotifyPropertyChanged
        {
            private string name;
            private string uri;
            private string status;
            private bool dnsResolv;

            public string Name
            {
                get => name;
                set
                {
                    if (name != value)
                    {
                        name = value;
                        OnPropertyChanged(nameof(Name));
                    }
                }
            }

            public string URI
            {
                get => uri;
                set
                {
                    if (uri != value)
                    {
                        uri = value;
                        OnPropertyChanged(nameof(URI));
                    }
                }
            }

            public string Status
            {
                get => status;
                set
                {
                    if (status != value)
                    {
                        status = value;
                        OnPropertyChanged(nameof(Status));
                    }
                }
            }

            public bool DnsResolv
            {
                get => dnsResolv;
                set
                {
                    if (dnsResolv != value)
                    {
                        dnsResolv = value;
                        OnPropertyChanged(nameof(DnsResolv));
                    }
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private List<URL> urls;
        public List<URL> URLList => urls;
        private int _counter = 0;

        private List<string> _dnsServers = new List<string>
        {
            "8.8.8.8",
            "18.194.186.127",
            "18.221.28.125",
            "18.221.142.58",
            "18.194.190.64"
        };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            urls = new List<URL>
            {
                new URL
                {
                    Name = "Printanista Hub Server",
                    URI = "https://audit.winwin-audit.de/koehn",
                    Status = "●"
                },
                new URL
                {
                    Name = "Register Server",
                    URI = "match.pf-d.ca",
                    Status = "●",
                    DnsResolv = true
                },
                new URL
                {
                    Name = "Update Server",
                    URI = "https://updates.printfleetcdn.com/dca-pulse/latest.json",
                    Status = "●"
                },
                new URL
                {
                    Name = "GttData2",
                    URI = "https://www.gttechonline.com/secured/gttservice2/gttdata2.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "LicenseActivator",
                    URI = "https://www.gttechonline.com/secured/licensingex/LicenseActivator.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "BillingReportSvc",
                    URI = "https://sbs.fmaudit.com/BillingReportService/BillingReportSvc.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "SBSSupplierService",
                    URI = "https://sbs.fmaudit.com/SBSSupplierService/SBSSupplierService.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "NormalizationData",
                    URI = "https://sbs.fmaudit.com/Normalization/NormalizationData.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "ReportsService",
                    URI = "https://sbs.fmaudit.com/ReportsService2/ReportsService.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "AuditService",
                    URI = "https://sbs.fmaudit.com/AuditService20/AuditService.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "sbsdata",
                    URI = "https://sbs.fmaudit.com/sbsservice/sbsdata.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "sbsdata2",
                    URI = "https://sbs.fmaudit.com/sbsservice2/sbsdata2.asmx",
                    Status = "●"
                },
                new URL
                {
                    Name = "ModelMatch",
                    URI = "https://modelmatch.printfleetcentral.com",
                    Status = "●"
                }
            };
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await CheckUrlsAsync();
        }

        private async Task CheckUrlsAsync()
        {
            var client = new HttpClient();

            foreach (var url in URLList)
            {
                if (url.DnsResolv)
                    await CheckUrlAsync(url, _dnsServers);
                else
                    await CheckUrlAsync(client, url);
            }

            StatusText.Text = $"{_counter} von {URLList.Count} erreichbar.";
        }

        private async Task CheckUrlAsync(HttpClient client, URL url)
        {
            try
            {
                var response = await client.GetAsync(url.URI);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    UpdateStatus(url, "200 OK");
                    _counter++;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    UpdateStatus(url, "404 OK");
                    _counter++;
                }
                else
                {
                    UpdateStatus(url, ((int)response.StatusCode).ToString() + " " + response.ReasonPhrase);
                }
            }
            catch (HttpRequestException e)
            {
                UpdateStatus(url, "Error: " + e.Message);
            }
        }

        private async Task CheckUrlAsync(URL url, List<string> dnsServers)
        {
            foreach (var dnsServer in dnsServers)
            {
                var resolved = await CheckDnsAsync(url.URI, dnsServer);
                if (resolved)
                {
                    url.Status = $"Resolved";
                    _counter++;
                    return;
                }
            }
            url.Status = "Not resolved";
        }

        private async Task<bool> CheckDnsAsync(string domain, string dnsServer)
        {
            try
            {
                var lookup = new LookupClient(IPAddress.Parse(dnsServer));
                var result = await lookup.QueryAsync(domain, QueryType.ANY);
                return result.Answers.Count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error querying DNS server {dnsServer}: {ex.Message}");
                return false;
            }
        }

        private void UpdateStatus(URL url, string status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                url.Status = status;
            });
        }
    }
}
