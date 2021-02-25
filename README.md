# Phantasma Docs
Documentation for Phantasma development

## How to run the documentation site locally

1. Clone or download this [reposity](https://github.com/phantasma-io/PhantasmaDocs/archive/master.zip) into your machine
2. Download the latest binaries from the [releases](https://github.com/phantasma-io/PhantasmaDocs/releases)
3. Create a "bin" folder inside the reposity folder, then unzip the binaries there
4. Run Docs.exe if using Windows (otherwise use 'dotnet run Docs.dll' in the terminal)
5. Open your browser and open the [localhost](http://localhost) URL

## How to edit an existing section topic

1. Find the folder for the section inside the ["en" folder](Frontend/docs/en)
2. Find the .html file that matches the topic
4. Edit the contents then press the Reload button to see the changes


## How to add a new section

1. Edit [sections.txt](Frontend/docs/sections.txt) and add a new line there with the new section title and [icon](https://fontawesome.com/icons?d=gallery&p=2&m=free)
2. Calculate the id of the new section by converting the title to lowercase and replacing any space with an underscore (eg: Smart Contract becomes smart_contract)
3. Create a new folder inside the ["en" folder](Frontend/docs/en), where the name of the new folder is the section id (from previous step)
4. Create several .html files as required with topics for that section. Make sure the .html files follow the format of number_title.html (the number is used for ordering them)

## How to add support for new language translations

1. Edit [languages.txt](Frontend/docs/languages.txt) and add a new line there with the new language title and [code](https://en.wikipedia.org/wiki/ISO_639-1)
2. Duplicate the ["en" folder](Frontend/docs/en), rename it to the new language code and start translating the files
3. When translation is finished, submit it as [PR](https://github.com/phantasma-io/PhantasmaDocs/pulls)
