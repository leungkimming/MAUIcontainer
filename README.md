# Run Blazor wasm in MAUI WebView [GitHub](https://github.com/leungkimming/MAUIcontainer)
## Goal
Our goal is to freely load and run any Blazor wasm App hosted in any host within a MAUI native Android/IOS App in such a way that Blazor wasm Apps can have bi-directional communicaiton with the MAUI native App for functions, such as push notificaiton, Authentication, etc.
## Why
* Deploying native App is costly while deploying Blazor wasm is relatively cheap.
* We can design a relatively stable MAUI App with generic, shared native features. Once deployed, it can load and run any Blazor wasm App hosted in any host.
* We can easily develop, test and debug Blazor wasm Apps in Browsers and no need to test in simulators, except only when testing native features.
* We can develop Android/IOS native functions in C# and no need to switch to Objective C/Java.
## How
* I need to thank Nathaniel Moschkin for his great project [mauiIbvieIxample](https://github.com/nmoschkin/mauiIbvieIxample), which gives me a good starting point to build my MAUIcontainer App.
* BlazorWebView is not suitable because we have to bundle all the Blazor code inside the MAUI App and we cannot freely load any Blazor from any host.
* MAUI WebView seems to be the only choice but WebView is just an HTML5 rendering engine without any UI implementations, such as navigation, file select for upload, download, etc. We have to implement them for both Android and IOS.
* I demonstrated some of these implementations in this PoC and below are the explainations.
* I will use our previous [Blazor wasm App PoC 'dotnet6EAA'](https://github.com/leungkimming/DotNet6EAA) as an example to run it in the PoC.
# Give it a go
## Basic Development Environment Setup
* Windows 10/11 Pro (Android emulators need hardware vitualization)
* VS2022 Community version 17.3
* Follow https://docs.microsoft.com/en-us/dotnet/maui/get-started/first-app?source=recommendations (Don't miss any step)
* Paired with a MAC book / mini / Air
* Download and run our previous [Blazor wasm App PoC 'dotnet6EAA'](https://github.com/leungkimming/DotNet6EAA)
* Download and able to run our MAUIcontainer PoC in both Android and IOS. You cannot run our Blazor wasm App inside MAUIcontainer yet.
## Additional Setup
* First, you cannot browse to https://localhost in your Android/IOS because localhost is the address of your Android/IOS itself, not that of your PC.
* I ended up using IP address (mime is 192.168.1.136) and you need to change it to that of your PC. In DOS prompt, run ipconfig.
* Add a firewall inbound rule to allow inbound traffic to reach IIS Express.
![img](./images/firewall.png)
* Update configuration of Blazor wasm App's IIS Express applicationhost.config<br>
![img](./images/IISExpress.png)
* Add binding to ANY IP address. ':44355:' means any IP address. The first 2 bindings for localhost are generated by VS2022. I need to add the third one.
```
      <site name="P1.API" id="2">
        <application path="/" applicationPool="P1.API AppPool">
          <virtualDirectory path="/" physicalPath="D:\....\DotNet6EAA\API" />
        </application>
        <bindings>
          <binding protocol="http" bindingInformation="*:28225:localhost" />
          <binding protocol="https" bindingInformation="*:44355:localhost" />
          <binding protocol="https" bindingInformation=":44355:" />
        </bindings>
      </site>
```
* Then, you need to run VS2022 as ADMINISTRATOR and run the dotnet6EAA solution. IIS Express need elevated privilege to bind to non localhost IP.
* If you deploy dotnet6EAA to a IIS server, then, you can simply use IIS server's host name in the MAUIcontainer App and no need to configure IIS Express.
* Note that you cannot debug dotnet6EAA while running in the MAUIcontainer because there is no communication betIen your Android/IOS emulator and VS2022. You shoud debug dotnet6EAA in the browser launched by VS2022.
* Change the IP address in MainPage.xaml.cs to that of your PC in the MAUIcontainer App.
```
public MainPage() {
    InitializeComponent();

    vm = new MainPageViewModel();
    BindingContext = vm;

    MyWebView.JavaScriptAction += MyWebView_JavaScriptAction;
    vm.UrlText = "https://192.168.1.136:44355/dotnet6EAA/";
```
* Now, you can try to load dotnet6EAA by clicking MAUIcontainer's "GO" button.
# The Solution
![img](./images/solution.png)
## Architecture Design
* Please read [MAUI Handlers - Customize Controls](https://docs.microsoft.com/en-us/dotnet/maui/user-interface/handlers/)
* HybridWebView is my customized WebView with extract events and properties.
* JavaScriptAction is helper class to call MAUI App methods from JavaScript
* Platforms folder contains Android/IOS native code and HybridWebViewHandler in C#.
* MainPage is main UI containing the customized WebView
* Login is handle Windows Authentication
* FileViewer acts as the second IOS WebView Tab while Android's WebView supports multiple windows. 
## Problems to be solved
For the description of how to solve the below problems, please read the respective wiki.
* Bi-directional communicaiton between Blazor App and MAUI App
* Windows Authentication
* Bypass SSL certificate error because of using IP address
* Click on a link and open a new browser Tab
* Download a Telerik document
* Using Telerik to upload a document
* Download from Telerik Report
