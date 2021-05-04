using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MediaCenter.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MediaCenterTests
{
    [TestClass]
    public class CreateRadioStationsLIst
    {
        [TestMethod]
        public void CreateList()
        {
            var stations = new List<RadioStation>();
            stations.Add(
                new RadioStation()
                {
                    Name = "SWR 1 BW",
                    URL = "http://mp3-live.swr.de/swr1bw_m.m3u",
                    LogoURL = "https://d3kle7qwymxpcy.cloudfront.net/images/broadcasts/07/41/2273/2/c175.png"
                });
            stations.Add(
                new RadioStation()
                {
                    Name = "B5 aktuell",
                    URL = "http://streams.br.de/b5aktuell_2.m3u",
                    LogoURL = "https://online-webradio.com/files/styles/80/public/logo/b5-aktuell.jpg"
                });

            var json = JsonSerializer.Serialize(stations);
            File.WriteAllText(@"d:\temp\radiostations.json", json);
        }
    }
}
