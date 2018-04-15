using Scraper.Models;
using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

namespace Scraper
{
    public class Scrape
    {
        static void Main(string[] args)
        {
            ProgramScrape();
        }
        
        //Read the html file to get Universities by State
        public static Dictionary<String, List<String>> UniversitiesByState()
        {
            Dictionary<String, List<String>> d = new Dictionary<String, List<String>>();

            string[] lines;
            lines = System.IO.File.ReadAllLines(@"/Users/JChase/Projects/Capstone/_git/website/GradSchooler/Scraper/ubystate.html");
            string statename = "";
            List<String> unis = new List<String>();
            var counter = 0;
            foreach( string line in lines)
            {
                // make sure we can index into the given string
                if (line.Length > 30)
                {
                    if (line.Substring(0, 11) == "<P><A NAME=")
                    {
                        if(statename != "")
                        {
                            d.Add(statename, unis); // add the schools of each state to the dictionary
                        }
                        string s = line.Substring(24); //returns a string that contains all chars past the 24th char
                        statename = s.Substring(0, s.IndexOf('<'));
                        unis = new List<String>();
                    }
                    else if(line.Substring(0, 12) == "<LI><A HREF="){
                        string university = lines[counter + 1].Trim();
                        university = university.Substring(0, university.IndexOf('<'));
                        unis.Add(university); //add university to list
                    }
                }
                counter++;
            }

            return d;

        }//UniversitiesByState


        public static void ProgramScrape()
        {
            //get the university state, name and city
            DBUtilities.DBUtilities db = DBUtilities.DBUtilities.Instance;
            List<University> unis = db.displayUniversities();

            string s = "";
            foreach (var u in unis)
            {
                s = u.name; //the university name 
                var sArray = s.Split(' '); //separate each word in the name into a string array
                string endURL = "";

                //loop over the array to fix the url with appropriate dashes
                for (int i = 0; i < sArray.Length; i++)
                { 
                    if (sArray[i].Equals("of"))
                    {
                        sArray[i] = "";
                    }
                    if(sArray[i].Contains("'"))
                    {
                        string st = sArray[i];
                        string str = ""; //string that has no apostrophes
                        var arr = st.Split('\''); //handles apostrophes

                        for(var item = 0; item < arr.Length; item++)
                        {
                            str += arr[item];
                        }


                        sArray[i] = str; //put the correct string back into the array
                        endURL += sArray[i] + "-";
                    }
                    else
                    {
                        endURL += sArray[i] + "-";
                    }
                }//end for

                endURL += u.city.Replace(' ', '-'); //concatinate the city name to the university name for scraping purposes

                string url = "https://www.gradschools.com/graduate-schools-in-united-states/";
                url += u.state.Replace(' ', '-') + "/" + endURL + "?page=";
               
                ProgramScrape(url);

            }//foreach

            
        }//end ProgramScrape()

        //private helper method to scrape the programs
        private static void ProgramScrape(string url)
        {
            List<string> programs = new List<string>();
            bool empty = false;
            int pagenum = 0;
            //read the webpages and get the programs
            while (!empty)
            {
                int eq = url.IndexOf('=');
                url = url.Substring(0, eq + 1);
                url = url + "" + pagenum;
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(url);
                HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
                //check if valid url
                if (r.Address.OriginalString != "https://www.gradschools.com/")
                {
                    HtmlNodeCollection collection = doc.DocumentNode.SelectNodes("//*[@id='" + "eddy-listings" + "']/li/h3/a/span");
                    if (collection == null)
                    {
                        empty = true;

                    }
                    else
                    {
                        foreach (HtmlNode li in collection)
                        {

                            //check if the page has programs
                            programs.Add(li.InnerText);
                            Debug.WriteLine(li.InnerText);
                        }
                    }

                    pagenum++;
                }//if

            }//end while

   
        }//Program Scrape Method



    }// class
}// namespace


      

