# Workshop ATDD / BDD (Fil rouge)
- Remarque globale : 
    - Effectivement, les différents noms de tests utilisés ici ne correspondent pas à ce que je vous ai enseigné sous forme de Method_Scernario_Resultat. Mais ils devraient ! (Et un jour je prendrai le temps de corriger ça)

# Partie 1 : ATDD sur le jeu du serpent

## Installation :
- Les étapes d'installation de paquets sont décrites directement dans ce workshop. En cas de souci, veuillez demander de l'aide au formateur.
- Lancer la commande docker pour démarrer bWAPP en local : 
``` docker run -d -p 80:80 hackersploit/bwapp-docker ```
- Aller sur 127.0.0.1/install.php
- Cliquer sur "Click here to install bWAPP."

## Démarrage du projet :
- Créez un projet de tests MsTest depuis Visual Studio, dans l'idéal mettez-vous sur le .Net 8.0.
- Ajoutez les dossier suivants à votre projet :
    - BaseClasses : Contiendra les classes de base du projet
    - ComponentHelper : Contiendra les méthodes static pour simplifier nos interactions avec nos composants d'interface
    - Configuration : Permettra de lire dans un fichier de configuration (appsettings.json)
    - CustomExceptions :
    - Interfaces
    - Settings
    - Tests

## Lecture du fichier de configuration (appsettings.json)
- Créez un fichier appsettings.json, le format doit être json, voici un exemple :
```
{
  "SiteSettings": {
    "Browser": "Firefox",
    "username": "bee",
    "password": "bug",
    "Website": "127.0.0.1"
  }
}
```
- Faites un click droit sur ce fichier et choisissez l'option "Toujours copier" pour ce fichier.
- Créez un dossier ConfigTests dans Tests
- Créez une classe SettingTests, passez la en public, vous allez attribuer [TestClass] sur la classe. Il se peut que vous deviez faire cet import ```using Microsoft.VisualStudio.TestTools.UnitTesting;```
- Ajoutez y une méthode Init en [TestInitialize] pour initialiser le test avec ce code (voir code ci-dessous)
- Les packages suivants seront peut être à installer manuellement : 
    - Microsoft.Extensions.Configuration.Json
    - Microsoft.Extensions.Configuration.EnvironmentVariables
    - Microsoft.Extensions.Configuration.Binder
```
private SiteSettings settings;

[TestInitialize]
public void Init()
{
    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables()
        .Build();

    settings = config.GetRequiredSection(nameof(SiteSettings)).Get<SiteSettings>();
}
```
- Ajoutez également un test pour chaque élément du fichier de configuration GetBrowserFromConfig, GetUsernameFromConfig,...
```
        [TestMethod]
        public void GetBrowserFromConfig()
        {
            Console.WriteLine($"Browser = { settings.Browser }");
        }

        [TestMethod]
        public void GetUsernameFromConfig()
        {
            Console.WriteLine($"Username = { settings.Username }");
        }

        [TestMethod]
        public void GetPasswordFromConfig()
        {
            Console.WriteLine($"Password = { settings.Password }");
        }

        [TestMethod]
        public void GetWebsiteFromConfig()
        {
            Console.WriteLine($"Website = { settings.Website }");
        }
```
- Créez une classe ConfigReaderTests à laquelle vous allez attribuer [TestClass]
  - Le test à réaliser en [TestMethod] est GetSettingsKeysFromConfigReader
  ```
  [TestMethod]
  public void GetSettingsKeysFromConfigReader()
  {
    IConfig config = new ConfigReader();
    Console.WriteLine("Browser : " + config.GetBrowser());
    Console.WriteLine("Username : " + config.GetUsername());
    Console.WriteLine("Password : " + config.GetPassword());
    Console.WriteLine("Website : " + config.GetWebsite());
  }
  ```
- Ajoutez une classe de mapping "SiteSettings" dans le dossier Configuration. Cette classe contient des propriétés pour les 4 éléments appartenant à votre dossier settings (Exemple : ```public string Browser { get; set; }```)
- A savoir, vous allez devoir installer et référencer sur le projet le package NuGet suivant : Microsoft.Extensions.Configuration. (Pour avoir accès à IConfiguration & ConfigurationBuilder)
- A savoir également, vous allez avoir besoin de référencer également via NuGet les packages suivant :
    - Microsoft.Extensions.Configuration.Binder
    - Microsoft.Extensions.Configuration.EnvironmentVariables
    - Microsoft.Extensions.Configuration.Json
- Ajoutez un élément de type Interface nommé IConfig dans le dossier Interfaces, ce dernier propose des signatures pour récupérer chaque élément du fichier appsettings (Exemple public ```BrowserType GetBrowser();```)
- Ajouter une enum BrowserType dans le dossier Settings
```
    public enum BrowserType
    {
        Chrome,
        Firefox,
        InternetExplorer
    }
```
- Ajouter une classe NoSuitableDriverFound dans le dossier CustomExceptions
```
    public class NoSuitableDriverFound : Exception
    {
        public NoSuitableDriverFound(string message) : base(message) { }
    }
```
- Ajoutez une classe ConfigReader dans le dossier Configuration
- Cette classe doit hériter de IConfig !
    - Copiez le constructeur proposé et modifiez le chemin d'accès à votre JsonFile (attention, normalement ça ne devrait être noté que appsettings.json, mais j'ai eu quelques soucis de mon côté).
    ```
        private SiteSettings settings;

        public ConfigReader()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            settings = config.GetRequiredSection(nameof(SiteSettings)).Get<SiteSettings>();
        }

        public BrowserType GetBrowser()
        {
            string browser = settings.Browser;

            try
            {
                return (BrowserType)Enum.Parse(typeof(BrowserType), browser);
            }
            catch (ArgumentException)
            {
                throw new NoSuitableDriverFound("Aucun driver n'a été trouvé  : " + settings.Browser);
            }
        }

        public string GetUsername()
        {
            return settings.Username;
        }

        [... Completez ...]
        ```
    - Créez les méthodes pour récupérer le Browser, le Username, le Password et le Website de votre appsettings.
- Vous pouvez maintenant développer votre test de lecture des settings.
- (Nous reviendrons plus tard sur ce fichier de tests pour réaliser le tests de lecture de settings depuis notre futur ObjectRepository.Config contenant notre configuration)

## Ouverture du navigateur + Aller à l'url
Premièrement, créez un dossier de Tests NavigatorTests, ajoutez y une classe NavigatorTests attribuée avec [TestClass] & [TestMethod].
- Réalisez une méthode de test d'ouverture du navigateur + GoToUrl pour chaque Driver :
  - Chrome avec ChromeDriver()
  - Firefox avec FirefoxDriver()
  - InternetExporer avec InternetExplorerDriver()
- Les méthodes de tests ressembleront à ça (x3)
```
        [TestMethod]
        public void OpenChromeAndGoToHomePage()
        {
            IWebDriver driver = new ChromeDriver();
            IConfig config = new ConfigReader();
            driver.Navigate().GoToUrl(config.GetWebsite());
            driver.Close();
            driver.Quit();
        }
```
- Pour lancer vos tests sur chaque browser, vous devez installer les paquets :
  - Selenium.WebDriver
  - Selenium.WebDriver.Support
  - Selenium.WebDriver.ChromeDriver
  - Selenium.WebDriver.GeckoDriver
  - Selenium.WebDriver.IEDriver
- Vous devriez dès lors pouvoir lancer vos tests et les faire fonctionner !
- Ajoutez dans vos tests les appels pour Close & Quit.
- A noter, il se peut que vous ayez quelques soucis avec le test sur IE, si c'est le cas, ajoutez l'attribut [Ignore] sur ce test en particulier.

## Mise en place de notre base et démarrage de l'assembly avec un ObjectRepostory valorisé
- Créez une classe ObjectRepository (Dossier BaseClasses) avec deux propriétés static : IConfig Config et IWebDriver Driver (avec un { get; set; } classique)
```
    public class ObjectRepository
    {
        public static IConfig Config { get; set; }
        public static IWebDriver Driver { get; set; }
    }
```
- Créez ensuite une classe BaseClass (Dossier BaseClasses), avec l'attribut [TestClass]. Cette classe doit contenir les méthodes static pour récupérer les différents Driver (GetChromeDriver, GetFirefoxDriver,...). Ainsi qu'une méthode InitWebDriver(TestContext tc) attibuée en [AssemblyInitialize] permettant d'initialiser votre ObjectRepository.Config et votre ObjectRepository.Driver. Ceci doit lire votre fichier de config et démarrer le bon Driver (Vous pouvez faire une enum comme dans l'exemple, mais ce n'est pas obligatoire)
```
    [TestClass]
    public class BaseClass
    {
        public static IWebDriver GetChromeWebDriver()
        {
            IWebDriver driver = new ChromeDriver();
            return driver;
        }

        public static IWebDriver GetFirefoxWebDriver()
        {
            IWebDriver driver = new FirefoxDriver();
            return driver;
        }

        public static IWebDriver GetInternetExplorerWebDriver()
        {
            IWebDriver driver = new InternetExplorerDriver();
            return driver;
        }

        [AssemblyInitialize]
        public static void InitWebDriver(TestContext tc)
        {
            ObjectRepository.Config = new ConfigReader();

            switch (ObjectRepository.Config.GetBrowser())
            {
                case BrowserType.Chrome:
                    ObjectRepository.Driver = GetChromeWebDriver();
                    break;

                case BrowserType.Firefox:
                    ObjectRepository.Driver = GetFirefoxWebDriver();
                    break;

                case BrowserType.InternetExplorer:
                    ObjectRepository.Driver = GetInternetExplorerWebDriver();
                    break;
            }

            ObjectRepository.Driver.Navigate().GoToUrl(ObjectRepository.Config.GetWebsite());
        }

        [AssemblyCleanup]
        public static void TearDownWebDriver()
        {
            if (ObjectRepository.Driver != null)
            {
                ObjectRepository.Driver.Close();
                ObjectRepository.Driver.Quit();
            }
        }
    }
```
- Cette classe doit contenir les méthodes static pour GetXDriver() avec X étant Chrome, Firefox & InternetExplorer.
- Vous pouvez alors utiliser ObjectRepository.Driver.Navigate().GoToUrl(ObjectRepository.Config.GetWebsite()) pour accéder à l'url de votre fichier de config.
- Ajoutez également une méthode en [AssembyCleanup] permettant de Close & Quit le Driver si ObjectRepository.Driver != null.
- A cette étape, le lancement du navigateur, le déplacement vers l'url et la fermeture du driver est réalisée à chaque démarrage et fermeture de notre projet.
- **Revenons à notre ConfigReaderTests** et ajoutons y le test pour accéder aux config depuis ObjectRepository.Config.
- Modifiez le test (si ce n'est pas déjà fait) pour accéder au site via ObjectRepository.Config et non en créant un objet ConfigReader.
- Créez un test dans NavigatorTests "OpenConfigBrowserWithBaseClass" affichant juste ObjectRepository.Config.GetWebsite(). (ça mange pas de pain de rajouter ce petit test)

## Gestion de la Navigation et du titre via Helper
- Ajoutez un dossier PageNavigationTests et créez une classe PageNavigationTests avec les attributs de classe MsTest habituels (TestClass, TestMethod).
- Développez 4 méthodes pour tester votre navigation :
  - OpenUrlFromDriver()
  - OpenUrlFromObjectRepository()
  - OpenUrlFromObjectRepositoryAndGetTitle()
  - OpenUrlFromObjectRepositoryAndGetTitleFromPageHelper()
- Cette étape permet de comprendre ce que l'on va faire dans ComponentHelper, elle n'est cependant pas obligatoire (Mais pour ce workshop elle l'est !)
- Le premier test devrait déjà fonctionner
- Pour le second, il faut mettre en place un NavigationHelper, qui nous servira pour nous faciliter la tâche quant à l'utilisation du ObjectRepository.Driver. Ce NavigationHelper doit contenir une méthode static void NavigateToUrl(string url) appelant ObjectRepository.Driver.Navigate().GoToUrl(url).
- Vous pouvez également faire une méthode NavigateToHomePage() qui utilise la même ligne de code que précédemment sauf que vous ciblez directement l'url du appsettings.json en paramètre avec ObjectRepository.Config.GetWebsite()
```
    public class NavigationHelper
    {
        public static void NavigateToUrl(string url)
        {
            ObjectRepository.Driver.Navigate().GoToUrl(url);
        }

        public static void NavigateToHomePage()
        {
            ObjectRepository.Driver.Navigate().GoToUrl(ObjectRepository.Config.GetWebsite());
        }
    }
```
- Pour le troisième, vous pouvez utiliser ObjectRepository.Driver.Title pour afficher le titre de la page.
```
    [TestClass]
    public class PageNavigationTests
    {
        [TestMethod]
        public void OpenPageFromDriver()
        {
            ObjectRepository.Driver.Navigate().GoToUrl(ObjectRepository.Config.GetWebsite());
        }

        [TestMethod]
        public void OpenPageFromObjectRepository()
        {
            NavigationHelper.NavigateToUrl(ObjectRepository.Config.GetWebsite());
        }

        [TestMethod]
        public void OpenPageFromObjectRepositoryAndGetTitle()
        {
            NavigationHelper.NavigateToUrl(ObjectRepository.Config.GetWebsite());
            Console.WriteLine(ObjectRepository.Driver.Title);
        }

        [TestMethod]
        public void OpenPageFromObjectRepositoryAndGetTitleFromPageHelper()
        {
            NavigationHelper.NavigateToUrl(ObjectRepository.Config.GetWebsite());
            Console.WriteLine(PageHelper.GetPageTitle());
        }
    }
```
- Pour le dernier test, vous allez devoir créer un PageHelper vous permettant de retourner le titre via une méthode static GetPageTitle()
```
    public class PageHelper
    {
        public static string GetPageTitle()
        {
            return ObjectRepository.Driver.Title;
        }

        public static string GetPageUrl()
        {
            return ObjectRepository.Driver.Url;
        }
    }
```

## Accéder aux composants Webs et Helper
### Accès à un élément
- Créez un dossier FindElementTest dans le dossier Tests
- Ajoutez une classe GenericHelper dans le dossier ComponentHelper
```
  public class GenericHelper
    {
        public static bool IsElementPresentOnce(By locator)
        {
            try
            {
                return ObjectRepository.Driver.FindElements(locator).Count == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool IsElementPresentAtLeastOnce(By locator)
        {
            try
            {
                return ObjectRepository.Driver.FindElements(locator).Count > 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static IWebElement GetElement(By locator)
        {
            if (IsElementPresentOnce(locator))
                return ObjectRepository.Driver.FindElement(locator);
            else
                throw new NoSuchElementException("Element not found" + locator.ToString());
        }
    }
```
- Ajoutez y une classe FindElementTest avec les attributs de classe MsTest. Ajoutez y 5 méthodes :
  - GetElementTests() : Qui va utiliser ObjectRepository.Driver.FindElement(By.LinkText("CONTACT")) (Ici changer le lien car ça n'est pas le bon projet : "New User" par exemple)
  - GetElementsTests() : qui va compter le nombre d'élements en utilisant ObjectRepository.Driver.FindElements(By.TagName("input"))
  - GetElementFromGenericHelper() : Qui va implémenter l'appel avec un GenericHelper utilisant ce qui a été fait précédemment
  - IsElementPresentOnce() : Qui va vérifier qu'un élément n'est présent qu'une fois FindElements().Count == 1
  - IsElementPresentOnceFromHelper(): Qui va utiliser GenericHelper pour réaliser l'opération précédente
```
    [TestClass]
    public class FindElementTests
    {
        [TestMethod]
        public void GetElementTests()
        {
            try
            {
                ObjectRepository.Driver.FindElement(By.LinkText("CONTACT")); // Trouver un lien existant ("New User" par exemple)
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void GetElementsTests()
        {
            try
            {
                ReadOnlyCollection<IWebElement> elements = ObjectRepository.Driver.FindElements(By.TagName("input"));
                Console.WriteLine(elements.Count);
            }
            catch (NoSuchElementException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        [TestMethod]
        public void GetElementFromGenericHelper()
        {
            Assert.IsNotNull(GenericHelper.GetElement(By.LinkText("CONTACT")));
        }

        [TestMethod]
        public void IsElementPresentOnce()
        {
            Assert.IsTrue(ObjectRepository.Driver.FindElements(By.LinkText("CONTACT")).Count == 1);
        }

        [TestMethod]
        public void IsElementPresentOnceFromGenericHelper()
        {
            Assert.IsTrue(GenericHelper.IsElementPresentOnce(By.LinkText("CONTACT")));
        }
    }
```

### Cliquer sur un lien
- A partir d'ici, je vous laisse essayer de réaliser les éléments par vous-même !
- Ajoutez également y une classe LinkTests avec les attributs de classe MsTest. Ajoutez y 2 méthodes :
  - ClickLinkTest() utilisant le GetElement précédent de GenericHelper puis en appelant la méthode Click sur cet élément
  - ClickLinkFromHelperTest() utilisant un LinkHelper pour faciliter l'opération précédente
- Vous allez avoir besoin d'un LinkHelper dans ComponentHelper, implémentez la méthode ```public static void ClickLink(By locator)``` en utilisant votre GenericHelper et la méthode Click du WebElement retourné.

### Ecrire dans une TextBox
- Ajoutez une classe de test MsTest "TextBoxTests" dans WebElementsTests avec les méthodes :
  - TextBoxTest()
  - TextBoxFromHelperTest()
  - ClearTextBoxTest()
- Pensez à vous mettre à la base sur une page qui contient le formulaire de connexion (login.php)
- Chaqué métode doit utiliser GenericHelper pour récupérer un élément (By.Id("login"),By.Id("password"))
- Sur l'élément, vous pouvez utiliser la méthode SendKeys(string key) pour ajouter une valeur à la textbox.
- Le dernier test quant à lui utilise Clear()
- Vous devez également créer un TextBoxHelper dans ComponentHelper pour gérer ça de manière transparante (on commence à avoir l'habitude normalement) Cette classe doit implémenter deux méthodes : ``` public static void TypeInTextBox(By locator, string text) ``` & ``` public static void ClearTextBox(By locator) ```

### Cliquer sur un boutton
- Ajoutez une classe de test MsTest "ButtonTests" dans WebElementsTests avec les méthodes :
  - ClickOnButtonTest()
  - ClickOnButtonFromHelperTest()
- Pour chaque méthode utilisez le TextBoxHelper pour remplir By.Id("p1") et By.Id("p2")
- Récupérez l'élément Button avec GenericHelper.GetElement(By.Id)
- Appelez la méthode Click() de l'élément.
- Vous devez également créer un ButtonHelper dans ComponentHelper pour gérer ça de manière transparante (on commence à avoir l'habitude normalement)

### Vérifier la page accédée
- Plusieurs méthodes s'offrent à vous, je vous laisse réaliser un test permettant de vérifier la page accéder, modifier / ajouter le helper correspondant et réaliser le test avec le helper.

## Tips :
- Si vous avez des soucis de navigation lors de vos tests, vous pouvez utiliser cet initialiseur de tests pour chacun de vos fichier tests nécessitant de démarrer sur la page d'accueil :
```
  [TestInitialize]
  public void Init()
  {
      NavigationHelper.NavigateToHomePage();
  }
```


**Première partie ATDD terminée !**

# Seconde partie : Behavior Driven Development
## Création du projet SpecFlow, pour faire du Gherkin
- Pour cette partie, nous allons devoir créer un nouveau projet. Celui-ci va nécessiter quelques réglages de nos packages (de nos deux projets) pour pouvoir fonctionner.
- Lors de la création de ce projet, choisissez le type de projet "SpecFlow"
- Choisissez "MsTest" et .Net 8.0 comme Framework de test cible (ou plus élevé si possible)
- Vous devez également ajouter les packages Nuget suivants :
  - SpecFlow
  - SpecFlow.MsTest
  - SpecFlow.Tools.MsBuild.Generation
  - SpecFlow.Plus.LivingDocPlugin
- Packages optionnels : (A minima le GeckoDriver si vous testez sur Firefox)
  - Selenium.WebDriver
  - Selenium.WebDriver.Support
  - Selenium.WebDriver.ChromeDriver
  - Selenium.WebDriver.GeckoDriver
  - Selenium.WebDriver.IEDriver
- Référencez votre projet ATDD (le précédent) sur ce projet SpecFlow. (Clic droit, ajouter référence de projet)
- A ce stade, la génération globale de la solution devrait toujours fonctionner, vos tests ATDD également.
- Dans ce projet, vous aurez besoin de 3 dossiers :
  - Features
  - Hooks
  - Steps
- Il se peut qu'il y ai des erreurs lors du build du projet à ce stade, si c'est le cas, supprimez la référence de projet, regénérez la solution globale et corrigez les erreurs (souvent, il suffira de supprimer ce qui pose souci à la génération dans le fichier .csproj ouvert lors du click sur l'erreur)
- En cas de souci, appelez-moi.

## Ajouter votre fichier FeatureFile de scénario en Gherkin
- Pour cette étape il faut d'abord installer l'extension Visual Studio pour SpecFlow, cliquez sur "extensions" => "Gérer les extensions" puis recherchez et ajoutez "SpecFlow for Visual Studio 2022".
- Vous devrez redémarrer votre instance de Visual Studio pour que les modifications se fassent (N'oubliez pas de cliquer sur "Modify")
- Relancer votre solution
- Clic droit sur le dossier FeatureFile du projet SpecFlow, ajoutez un nouvel élément et recherchez "Feature File for SpecFlow". Prenez le classique, mais remarquez qu'il est aussi possible de le faire dans plusieurs langues.
- Nommez ce fichier "StartAGame.feature", par exemple.
- Renommez la Feature comme vous le souhaitez
- Gardez un seul scénario, celui ci sera un scénario de démarrage d'une partie.
- A vous d'écrire le fichier FeatureFile comme vous l'imaginez avec ce que l'on a vu concernant le Gherkin et ses mots clés (Given, And, When, Then,...)
  - L'utilisateur est sur la page de d'accueil
  - On vérifie que les inputs p1 et p2 ainsi que le boutton submitPlayers existent
  - Quand les inuputs sont remplis et que le bouton avec l'id "submitPlayers" est cliqué
  - L'utilisateur doit être à la page choix de jeu (PageHelper.GetUrl() == Path.Combine(ObjectRepository.Website, "game")).

## Créer le fichier StepDefinition pour couvrir l'intégralité du FeatureFile
- Ici, vous pouvez faire un click droit dans votre fichier FeatureFile, puis cliquez sur "Generate Step Definitions" (Ou "Define Steps"). Le fichier qui sera généré va vous aider mais doit être retravaillé.
- Pour chaque Step, il vous suffit d'utiliser les Helper que vous avez mis en place lors de la partie ATDD afin de réaliser ce qui est demandé dans une step (selon votre définition des steps)
- A noter que certaines Steps nécessitent des Assert pour faire des vérifications
- Avant de pouvoir lancer vos tests, il va falloir mettre en place un petit tricks afin d'initialiser notre projet principal dans nos tests. Ce trick consiste à utiliser un Hook SpecFlow pour initaliser et cleanup notre environnement de tests (C'est l'équivalent de ce qu'on avait fait avec le BaseClass)
- Créez une nouvelle classe dans le dossier Hooks (SpecFlow) comme l'exemple ci-dessous :
```
[Binding]
    public class InitScenarioHook
    {
        [BeforeScenario]
        public static void InitScenario()
        {
            ObjectRepository.Config = new ConfigReader();

            switch (ObjectRepository.Config.GetBrowser())
            {
                case BrowserType.Chrome:
                    ObjectRepository.Driver = BaseClass.GetChromeWebDriver();
                    break;

                case BrowserType.Firefox:
                    ObjectRepository.Driver = BaseClass.GetFirefoxWebDriver();
                    break;

                case BrowserType.InternetExplorer:
                    ObjectRepository.Driver = BaseClass.GetInternetExplorerWebDriver();
                    break;
            }

            NavigationHelper.NavigateToUrl(ObjectRepository.Config.GetWebsite());
        }

        [AfterScenario]
        public static void TearDown()
        {
            if (ObjectRepository.Driver != null)
            {
                ObjectRepository.Driver.Close();
                ObjectRepository.Driver.Quit();
            }
        }
    }
```

## Lancer vos tests
- Lancez vos tests SpecFlow, normalement, ils ne devraient pas fonctionner.
- Vous devriez avoir une erreur "geckodriver.exe not found". Nous allons donc l'ajouter avec le package NuGet Selenium.WebDriver.GeckoDriver.
- Regénérez la solution puis relancez vos tests
- Si tout va bien, vos tests devraient fonctionner !
- A noter, si votre ATDD ne fonctionne plus, c'est un souci de doublon avec le package Selenium.WebDriver.GeckoDriver, à ce stade il vous suffit de supprimer le package NuGet dans votre solution ATDD (Donc dans la solution de la partie précédente du workshop !!)

## Refaites toutes les étapes précédentes afin de gérer un scénario de lancement complet de jeu
- Cliquez en boucle (OUI, c'est l'un des rare cas om l'on met une boucle dans un test !!) sur By.Id("new_turn")
- Vérifier que l'utilisateur se trouve bien sur la page endgame

- Fiou... On arrive au bout de ce workshop, enfin !
# WorkShop
