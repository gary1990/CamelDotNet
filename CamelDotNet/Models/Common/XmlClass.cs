using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CamelDotNet.Models.Common
{
    public class ProductTypeXml
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ClientXml
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class TestStationXml
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Process { get; set; }
    }

    [XmlRoot("Result")]
    public class TestStationListXml
    {
        public TestStationListXml()
        {
            Message = "true";
            TestStationXmls = new List<TestStationXml> { };
        }
        public string Message { get; set; }
        [XmlElement("TestStation")]
        public List<TestStationXml> TestStationXmls { get; set; } 
    }

    public class TestItemXml
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Category { get; set; }
    }

    [XmlRoot("Result")]
    public class TestConfigListXml
    {
        public TestConfigListXml()
        {
            Message = "true";
            TestConfigXmls = new List<TestConfigXml> { };
        }
        public string Message { get; set; }
        [XmlElement("TestConfig")]
        public List<TestConfigXml> TestConfigXmls { get; set; }
    }

    [XmlType("TestConfig")]
    public class TestConfigXml
    {
        public TestConfigXml()
        {
            TestItemConfigXmls = new List<TestItemConfigXml> { };
        }
        [XmlElement("ProductType")]
        public ProductTypeXml ProductTypeXml { get; set; }
        [XmlElement("Client")]
        public ClientXml ClientXml { get; set; }
        [XmlElement("TestItemConfig")]
        public List<TestItemConfigXml> TestItemConfigXmls { get; set; }
    }

    public class TestItemConfigXml
    {
        public TestItemConfigXml()
        {
            PerConfigXmls = new List<PerConfigXml> { };
        }
        [XmlElement("TestItem")]
        public TestItemXml TestItemXml { get; set; }
        public string VersionDate { get; set; }
        [XmlElement("PerConfig")]
        public List<PerConfigXml> PerConfigXmls { get; set; }
    }

    public class PerConfigXml
    {
        public int? Channel { get; set; }
        public int? Trace { get; set; }
        public decimal StartF { get; set; }
        public decimal StopF { get; set; }
        public decimal ScanPoint { get; set; }
        public decimal? ScanTime { get; set; }
        public decimal? TransportSpeed { get; set; }
        public decimal? FreqPoint { get; set; }
        public decimal LimitLine { get; set; }
    }

    [XmlRoot("Result")]
    public class SingleResultXml
    {
        public string Message { get; set; }
    }
}