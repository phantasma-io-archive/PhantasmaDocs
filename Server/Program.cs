using System;
using System.Threading;

using LunarLabs.WebServer.Core;
using LunarLabs.WebServer.HTTP;
using System.Collections.Generic;
using LunarLabs.WebServer.Templates;
using System.IO;
using System.Globalization;
using System.Linq;

namespace Phantasma.Docs
{
    public class Entry
    {
        public int order;
        public string link;
        public string content;
        public string name;
    }

    public class Section
    {
        public string link;
        public string icon;
        public string title;
        public string intro;
        public List<Entry> entries;
    }

    public class Docs
    {
        public string language;
        public string code;
        public List<Section> sections;
    }

    public struct Topic
    {
        public string title;
        public string icon;
        public string ID;

        public Topic(string title, string icon)
        {
            this.title = title;
            this.icon = icon;
            this.ID = title.ToLower().Replace(" ", "_");
        }
    }

    class Program
    {
        const string LanguageHeader = "Accept-Language";

        static string DetectLanguage(HTTPRequest request)
        {
            if (request.headers.ContainsKey(LanguageHeader))
            {
                var languages = request.headers[LanguageHeader].Split(new char[] { ',', ';' });
                foreach (var lang in languages)
                {
                    string code;
                    if (lang.Contains("-"))
                    {
                        code = lang.Split('-')[0];
                    }
                    else
                    {
                        code = lang;
                    }

                    if (LocalizationManager.HasLanguage(code))
                    {
                        return code;
                    }
                }
            }

            return "en";
        }

        static Dictionary<string, Docs> _docs = new Dictionary<string, Docs>();

        private static string BeautifyName(string name)
        {
            name = name.Replace("example ", "example: ", StringComparison.OrdinalIgnoreCase);
            return name.Replace("library ", "library: ", StringComparison.OrdinalIgnoreCase);
        }

        private static Docs LoadDocs(string path, string code, string name, List<Topic> topics)
        {
            var docs = new Docs();
            docs.code = code;
            docs.language = name;
            docs.sections = new List<Section>();

            var docFolder = path + code + Path.DirectorySeparatorChar;

            foreach (var topic in topics)
            {
                var section = new Section();
                section.icon = topic.icon;

                // HACK makes some menu names more readable...
                var topicName = BeautifyName(topic.title);

                section.title = topicName;
                section.link = topic.ID;
                section.entries = new List<Entry>();
                Entry tempEntry;

                var introPath = docFolder + topic.ID + ".html";
                if (File.Exists(introPath))
                {
                    section.intro = File.ReadAllText(introPath);
                }

                var topicPath = docFolder + topic.ID + Path.DirectorySeparatorChar;

                if (Directory.Exists(topicPath))
                {
                    var files = Directory.GetFiles(topicPath, "*.html");

                    foreach (var file in files)
                    {
                        var entry = new Entry();

                        var temp = Path.GetFileNameWithoutExtension(file).Split('_', 2);

                        entry.order = int.Parse(temp[0]);

                        var entryName = temp[1].Replace("_", " ");

                        // HACK makes some menu names more readable...
                        entryName = BeautifyName(entryName);

                        entry.name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(entryName);
                        entry.link = section.link + "-" + temp[1];

                        entry.content = File.ReadAllText(file);

                        section.entries.Add(entry);
                    }

                    for (int i = 0; i < section.entries.Count; i++)
                    {

                        for (int j = i+1; j < section.entries.Count; j++)
                        {
                            if (section.entries[i].order > section.entries[j].order)
                            {
                                tempEntry = section.entries[i];
                                section.entries[i] = section.entries[j];
                                section.entries[j] = tempEntry;
                            }
                        }
                    }
                }

                docs.sections.Add(section);
            }

            return docs;
        }

        static void LoadAllDocs(string docPath)
        {
            var sectionsFile = docPath + "sections.txt";
            if (!File.Exists(sectionsFile))
            {
                throw new Exception("Could not find file: " + sectionsFile);
            }

            var topicEntries = File.ReadAllLines(sectionsFile);
            var topics = new List<Topic>();
            foreach (var line in topicEntries)
            {
                var temp = line.Split(',');
                if (temp.Length != 2)
                {
                    continue;
                }

                var name = temp[0];
                var icon = temp[1];

                topics.Add(new Topic(name, icon));
            }

            var langFile = docPath + "languages.txt";
            if (!File.Exists(langFile))
            {
                throw new Exception("Could not find file: " + langFile);
            }

            var languageEntries = File.ReadAllLines(langFile);
            _docs.Clear();
            foreach (var line in languageEntries)
            {
                var temp = line.Split(',');
                if (temp.Length != 2)
                {
                    continue;
                }

                var code = temp[0];
                var name = temp[1];

                _docs[code] = LoadDocs(docPath, code, name, topics);
            }
        }

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            if (args.Length == 0)
            {
                args = new string[] { "--path=" + Path.GetFullPath("../Frontend")};
            }

            var settings = ServerSettings.Parse(args);

            var server = new HTTPServer(settings, ConsoleLogger.Write);

            var templateEngine = new TemplateEngine(server, "views");

            Console.WriteLine("Frontend path: " + settings.Path);

            /*var locFiles = Directory.GetFiles("Localization", "*.csv");
            foreach (var fileName in locFiles)
            {
                var language = Path.GetFileNameWithoutExtension(fileName).Split("_")[1];
                LocalizationManager.LoadLocalization(language, fileName);
            }*/

            var docPath = settings.Path + "docs" + Path.DirectorySeparatorChar;

            LoadAllDocs(docPath);

            Func<HTTPRequest, Dictionary<string, object>> GetContext = (request) =>
            {

                var context = new Dictionary<string, object>();

                context["available_languages"] = LocalizationManager.Languages;

                var langCode = request.session.GetString("language", "auto");

                if (langCode == "auto")
                {
                    langCode = DetectLanguage(request);
                    request.session.SetString("language", langCode);
                }

                context["current_language"] = LocalizationManager.GetLanguage(langCode);

                var docs = _docs["en"];

                context["sections"] = docs.sections;

                return context;
            };

            server.Get("/language/{code}", (request) =>
            {
                var code = request.GetVariable("code");

                if (LocalizationManager.GetLanguage(code) != null)
                {
                    request.session.SetString("language", code);
                }

                return HTTPResponse.Redirect("/");
            });

            server.Get("/", (request) =>
            {
                var context = GetContext(request);
                return templateEngine.Render(context, "main");
            });

            server.Get("/reload", (request) =>
            {
                LoadAllDocs(docPath);
                return HTTPResponse.Redirect("/");
            });

            server.Run();

            bool running = true;

            Console.CancelKeyPress += delegate {
                server.Stop();
                running = false;
            };

            while (running)
            {
                Thread.Sleep(500);
            }
        }
    }
}
