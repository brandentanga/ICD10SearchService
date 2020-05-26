using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ICD10SearchService.Models
{
    [XmlType(TypeName = "ICD10CM.tabular")]
    public class Icd10CM
    {
        [XmlElement("version")]
        public string Version;

        [XmlElement("introduction")]
        public Introduction Intro;

        [XmlElement("chapter")]
        public List<Chapter> Chapters;

        [XmlType(TypeName = "introduction")]
        public class Introduction
        {
            [XmlElement("introSection")]
            public List<IntroSection> IntroSections;
        }

        [XmlType(TypeName = "introSection")]
        public class IntroSection
        {
            [XmlElement("title")]
            public string Title;
        }

        public class Chapter
        {
            [XmlElement("name")]
            public string Name;

            [XmlElement("desc")]
            public string Desc;

            [XmlElement("includes")]
            public Includes Includes;

            [XmlElement("section")]
            public List<Section> Sections;
        }
        public class Includes
        {
            [XmlElement("note")]
            public string Note;
        }

        public class Section
        {
            [XmlElement("desc")]
            public string Desc;

            [XmlElement("includes")]
            public Includes Includes;

            [XmlElement("diag")]
            public List<Diagnosis> Diagnoses;
        }

        public class Diagnosis
        {
            [XmlElement("name")]
            public string Name;

            [XmlElement("desc")]
            public string Desc;

            [XmlElement("diag")]
            public List<Diagnosis> Diagnoses;
        }

    }
}
