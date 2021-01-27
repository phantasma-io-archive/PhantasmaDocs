using System;
using System.Linq;
using System.IO;
using System.Threading;

using LunarLabs.WebServer.Core;
using LunarLabs.WebServer.HTTP;
using System.Collections.Generic;
using LunarLabs.WebServer.Templates;
using Phantasma.Core.Types;
using Phantasma.Numerics;
using Phantasma.VM.Utils;
using Phantasma.VM;
using Phantasma.Storage;
using Phantasma.Domain;

namespace Phantasma.Docs
{
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

        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            var settings = ServerSettings.Parse(args);

            var server = new HTTPServer(settings, ConsoleLogger.Write);

            var templateEngine = new TemplateEngine(server, "views");

            /*var locFiles = Directory.GetFiles("Localization", "*.csv");
            foreach (var fileName in locFiles)
            {
                var language = Path.GetFileNameWithoutExtension(fileName).Split("_")[1];
                LocalizationManager.LoadLocalization(language, fileName);
            }*/


            int refreshRate = 5;

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
