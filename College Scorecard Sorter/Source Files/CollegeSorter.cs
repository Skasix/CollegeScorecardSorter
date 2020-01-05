using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class CollegeSorter
{
	public static string json;
	public static JObject schools;
	public static string directory = Directory.GetCurrentDirectory() + @"\schools";

	static void Main()
	{
		Console.WriteLine("This program is still a work in progress and features will progressively be added.");
		Console.WriteLine("Currently, the program filters colleges by specific majors and display the median earnings.");
		Console.Write("Please enter the first four CIP digits of your selected major: ");
		urlPicker.UpdateFilter("latest.programs.cip_4_digit.code=" + urlPicker.validInt());
		Console.Clear();

		urlPicker.getAPIKey();
		urlPicker.UpdateUrl();
		using (WebClient webClient = new WebClient())
		{
			schools = DeNullifier(getJSON());

			//Merges other pages into json if there are more pages
			int schoolsLeft = (int)schools.First.First.First.First - 100;
			while (schoolsLeft > 0)
			{
				//Update API Request Url to current page
				int pageNum = int.Parse(urlPicker.PageCount.Substring(urlPicker.PageCount.IndexOf("=") + 1)) + 1;
				urlPicker.UpdatePageCount(pageNum);
				urlPicker.UpdateUrl();

				//Merge the current page's JSON
				schools.Merge(DeNullifier(getJSON()), new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });

				schoolsLeft -= 100;
			}
		}
		Console.WriteLine("All pages have been processed. Results will be displayed at the end.");
		Console.WriteLine("Would you like to save a json of the results? true/false");
		bool createFile;
		while (!bool.TryParse(Console.ReadLine(), out createFile))
			bool.TryParse(Console.ReadLine(), out createFile);
		if (createFile)
		{
			bool replaceFile = true;
			Console.WriteLine("Would you like to replace schools.json? Recommended setting is false. true/false");
			while (!bool.TryParse(Console.ReadLine(), out replaceFile))
				bool.TryParse(Console.ReadLine(), out replaceFile);
			createJSON(replaceFile);
		}

		Console.WriteLine(schools);
		Console.WriteLine("To exit, press the power button.");
		Console.ReadKey();
	}

	//Parameter indicates whether or not to replace schools.json or create more iterations ex.(schools34.json)
	static void createJSON(bool replaceExisting)
	{
		string fileNum = "";

		//Changes the file name if necessary to create fresh files. 
		if (!replaceExisting)
		{
			//Continuously adds number until a fresh file can be made
			while (File.Exists(directory + fileNum + ".json"))
			{
				for (int i = 0; i < 10; i++)
				{
					if (!File.Exists(directory + fileNum + i + ".json"))
					{
						fileNum += i;
						break;
					}
					else if (i == 9 && !File.Exists(directory + i))
						fileNum += 0;
				}
			}

		}

		using (StreamWriter file = File.CreateText(directory + fileNum + ".json"))
		using (JsonTextWriter writer = new JsonTextWriter(file))
		{
			writer.Formatting = Formatting.Indented;
			schools.WriteTo(writer);
		}
		Console.WriteLine("Json file has been written to {0} successfully.", directory + fileNum + ".json");
	}

	//Returns an indented JSON file of current page
	static JObject getJSON()
	{
		using (WebClient webClient = new WebClient())
		{
			//JSON File indentation property
			JsonLoadSettings indented = new JsonLoadSettings();
			indented.LineInfoHandling = LineInfoHandling.Load;

			//Downloads JSON as string, prettifies it then converts it into a JObject
			json = webClient.DownloadString(urlPicker.Url);
			json = JValue.Parse(json).ToString(Formatting.Indented);
			Console.WriteLine("Page {0} processed.", urlPicker.PageCount.Substring(urlPicker.PageCount.IndexOf("=") + 1));
			return JObject.Parse(json, indented);
		}
	}

	static JObject DeNullifier(JObject inputJson)
	{
		List<JToken> nullMajors = new List<JToken>();

		//Each school in the results[] section
		foreach (var school in inputJson["results"])
		{
			//Each degree in the cip_4_digit section
			foreach (var degree in school["latest.programs.cip_4_digit"])
			{
				if (string.IsNullOrEmpty(degree["earnings.median_earnings"].Value<string>()))
				{
					nullMajors.Add(degree);
				}
			}
		}
		foreach (var result in nullMajors)
		{
			result.Remove();
		}

		List<JToken> nullSchools = new List<JToken>();
		foreach (var school in inputJson["results"])
		{
			if (!school["latest.programs.cip_4_digit"].HasValues)
				nullSchools.Add(school);
		}
		foreach (var school in nullSchools)
			school.Remove();

		return inputJson;
	}
}
