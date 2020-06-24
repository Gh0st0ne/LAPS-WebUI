﻿using Integrative.Lara;
using LAPS_WebUI.PageData;
using NLog;
using System;
using System.DirectoryServices;
using System.Threading.Tasks;
using LAPS_WebUI.Ressources;

namespace LAPS_WebUI.Pages
{
    [LaraPage(Address = "/login")]
    class Login : IPage
    {

        internal LoginData m_logindata = new LoginData();

        public Task OnGet()
        {

            var thisDocument = LaraUI.Page.Document;

            if (UserSession.LoggedIn)
            {
                LaraUI.Page.Navigation.Replace("/laps");
            }
            else
            {

                Bootstrap.AppendTo(thisDocument.Head);
                FontAwesome.AppendTo(thisDocument.Head);

                thisDocument.Body.AddClass("text-center");

                var builder = new LaraBuilder(thisDocument.Body);

                builder
                    .Push("div", "card mx-auto mt-5")
                        .Attribute("style", "width: 24rem;")
                        .Push("article", "card-body")
                            .Push("h4", "card-title text-center mb-4 mt-1")
                                .InnerText("Sign in")
                            .Pop()
                            .Push("i", "fas fa-user-circle fa-5x")
                            .Pop()
                            .Push("hr")
                            .Pop()
                            .Push("div", "alert alert-warning text-center")
                                .Attribute("id", "loginmessage")
                                .Attribute("role", "alert")
                                .FlagAttribute("hidden", true)
                            .Pop()
                            .Push("form")
                                .Attribute("id", "loginForm")
                                .On(new EventSettings
                                {
                                    EventName = "submit",
                                    PreventDefault = true,
                                    Handler = OnLoginSubmit,
                                    Block = true,
                                    BlockOptions = new BlockOptions
                                    {
                                        ShowHtmlMessage = "Please wait while loggin in..."
                                    }

                                })
                                .Attribute("action","/lol")
                                .Push("div", "form-group")
                                    .Push("div", "input-group")
                                        .Push("span", "input-group-text")
                                            .Push("i", "fa fa-user")
                                            .Pop()
                                        .Pop()
                                        .Push("input", "form-control")
                                            .Attribute("type", "email")
                                            .Attribute("placeholder", "E-Mail or login")
                                            .Attribute("name", "email")
                                            .BindInput("value", m_logindata, x => x.Username)
                                        .Pop()
                                    .Pop() // Input Group
                                .Pop() // Form-Group
                                .Push("div", "form-group")
                                    .Push("div", "input-group")
                                        .Push("span", "input-group-text")
                                            .Push("i", "fa fa-key")
                                            .Pop()
                                        .Pop()
                                        .Push("input", "form-control")
                                            .Attribute("type", "password")
                                            .Attribute("placeholder", "Password")
                                            .Attribute("name","password")
                                            .BindInput("value", m_logindata, x => x.Password)
                                        .Pop()
                                    .Pop() // Input Group
                                .Pop() // Form-Group
                                .Push("div", "form-group")
                                    .Push("button", "btn btn-primary btn-block")
                                        .Attribute("id", "loginbutton")
                                        .Attribute("type","submit")
                                        .InnerText("Login")
                                    .Pop() // Button
                                .Pop() // form-group
                            .Pop() //form
                        .Pop()
                    .Pop();
            }

            return Task.CompletedTask;
        }

        private Task OnLoginSubmit()
        {

            var loginMessage = LaraUI.Document.GetElementById("loginmessage");
            bool authResult = false;
            Logger m_log = LogManager.GetLogger("OnLogin");

            try
            {

                if (string.IsNullOrWhiteSpace(m_logindata.Username) || string.IsNullOrWhiteSpace(m_logindata.Password))
                {
                    throw new Exception("Empty fileds");
                }

                using (var domainEntry = new DirectoryEntry(string.Format("LDAP://{0}:{1}", Settings.ThisInstance.LDAP.Server, Settings.ThisInstance.LDAP.Port), m_logindata.Username, m_logindata.Password, Settings.ThisInstance.LDAP.UseSSL ? AuthenticationTypes.SecureSocketsLayer : AuthenticationTypes.None))
                {
                    if (domainEntry.NativeObject != null)
                    {
                        authResult = true;
                    }
                }
                
                if (!authResult)
                {
                    loginMessage.InnerText = "Login failed - Wrong Username or Password";
                    loginMessage.SetFlagAttribute("hidden", false);
                }
            }
            catch (DirectoryServicesCOMException ex)
            {
                loginMessage.InnerText = "Login failed";
                loginMessage.SetFlagAttribute("hidden", false);
                m_log.Warn("Login for User {0} failed => {1}", m_logindata.Username,ex.Message);
            }
            catch (Exception ex)
            {
                loginMessage.InnerText = "Login failed";
                loginMessage.SetFlagAttribute("hidden", false);
                m_log.Error(ex.Message);
            }
            finally
            {
               if (authResult)
               {
                    UserSession.LoggedIn = true;
                    UserSession.loginData = m_logindata;
                    LaraUI.Page.JSBridge.Submit(@"window.location.replace('/laps');");
               }

            }

            return Task.CompletedTask;
        }
    }
}