﻿using System;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using System.Linq;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Web;

using System.Web.Script.Serialization;

namespace NetworkAPI
{
    public class NetworkAPIMod : IUserMod
    {
        public string Name { get { return "Network API"; } }
        public string Description { get { return "This mod exposes the Cities: Skylines Data and Interfaces Through Sockets."; } }
    }

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode != LoadMode.NewGame && mode != LoadMode.LoadGame)
            {
                return;
            }

            // this seems to get the default UIView
            UIView uiView = UIView.GetAView();

            // example for adding a button

            // Add a new button to the view.
            var button = (UIButton)uiView.AddUIComponent(typeof(UIButton));

            // Set the text to show on the button.
            button.text = "Start Server";

            // Set the button dimensions.
            button.width = 200;
            button.height = 30;

            // Style the button to look like a menu button.
            button.normalBgSprite = "ButtonMenu";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.focusedBgSprite = "ButtonMenuFocused";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.textColor = new Color32(255, 255, 255, 255);
            button.disabledTextColor = new Color32(7, 7, 7, 255);
            button.hoveredTextColor = new Color32(7, 132, 255, 255);
            button.focusedTextColor = new Color32(255, 255, 255, 255);
            button.pressedTextColor = new Color32(30, 30, 44, 255);

            // Enable button sounds.
            button.playAudioEvents = true;

            //set button position
            button.transformPosition = new Vector3(0.8f, 0.95f);
            button.BringToFront();

            try
            {
                //get the names of any citizens in the city
                CitizenManager cm = CitizenManager.instance;

                // example for iterating through the structures
                int cCount = 0;
                int maxCCount = cm.m_citizenCount;

                Debug.Log ("Citizen maxCCount: " + maxCCount);

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Citizen maxCCount: " + maxCCount);

                for (int i = 0; i < maxCCount; i++) {
                    String c = cm.GetCitizenName((uint)i);
                    if (c != null && !c.Equals("")) {
                        cCount += 1;
                    }
                }

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Actual Citizens: " + cCount);

            } catch (Exception e) {
                Debug.Log("Error in detecting citizen names");
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }

        }

    }

    [ServiceContract]
    public interface IManagerService
    {
        [WebGet(UriTemplate = "managers", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagers();

        [WebGet(UriTemplate = "managers/{managername}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerTypes(string managername);

        [WebGet(UriTemplate = "managers/{managername}/{type}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        List<string> GetManagerProperties(string managername, string type);

        [WebGet(UriTemplate = "managers/{managername}/{type}/{propertyname}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [OperationContract]
        string GetManagerProperty(string managername, string type, string propertyname);
    }

    public class ManagerService : IManagerService
    {
        JavaScriptSerializer serializer;

        public ManagerService()
        {
            serializer = new JavaScriptSerializer();
        }

        public List<string> GetManagers()
        {
            List<string> managers = new List<string>();
            try
            {
                Assembly assembly = Assembly.Load("Assembly-CSharp");
                managers = assembly.GetTypes()
                    .Where(x => x.Name.IndexOf("Manager") > -1)
                    .Select(x => x.Name).ToList<string>();
            }
            catch (Exception e)
            {
                managers.Add(e.Message);
            }
            return managers;
        }

        public List<string> GetManagerTypes(string managername)
        {
            List<string> types = new List<string>();
            types.Add("members");
            types.Add("methods");
            types.Add("properties");
            types.Add("fields");
            types.Add("events");
            types.Add("nestedTypes");
            return types;
        }

        public List<string> GetManagerProperties(string managername, string type)
        {
            List<string> properties = new List<string>();
            try
            {
                Assembly assembly = Assembly.Load("Assembly-CSharp");
                Type t = assembly.GetType(managername);
                if (type == "properties")
                {
                    properties = t.GetProperties().Select(x => x.Name).ToList<string>();
                }
                else if (type == "methods")
                {
                    properties = t.GetMethods().Select(x => x.Name).ToList<string>();
                }
                else if (type == "members")
                {
                    properties = t.GetMembers().Select(x => x.Name).ToList<string>();
                }
                else if (type == "fields")
                {
                    properties = t.GetFields().Select(x => x.Name).ToList<string>();
                }
                else if (type == "events")
                {
                    properties = t.GetEvents().Select(x => x.Name).ToList<string>();
                }
                else if (type == "nestedTypes")
                {
                    properties = t.GetNestedTypes().Select(x => x.Name).ToList<string>();
                }
            }
            catch (Exception e)
            {
                properties.Add(e.Message);
            }
            return properties;
        }

        public Type getAssemblyType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }

        public object getInstance(string assemblyName, string typeName)
        {
            // get the instance of the manager here:
            Type t = getAssemblyType( assemblyName, typeName);
            PropertyInfo instancePropInfo = (PropertyInfo)t.GetMember("instance")[0];
            MethodInfo instanceMethodInfo = instancePropInfo.GetAccessors()[0];
            return instanceMethodInfo.Invoke(null, null);
        }

        public object getPropertyValue(string managername, string name)
        {
            object retObj;
            object manager = getInstance("Assembly-CSharp", managername);
            Type t = getAssemblyType("Assembly-CSharp", managername);
            MethodInfo mi = t.GetProperty(name).GetGetMethod();
            retObj = mi.Invoke(manager, null);
            return retObj;
        }

        public string GetManagerProperty(string managername, string type, string propertyname)
        {
            string returnString = "";
            try
            {
                Type t = getAssemblyType("Assembly-CSharp", managername);
                object manager = getInstance("Assembly-CSharp", managername);

                if (type == "properties")
                {
                    returnString += getPropertyValue(managername, propertyname);
                }
                else if (type == "methods")
                {
                    MemberInfo[] m = t.GetMember(propertyname);
                    foreach (ParameterInfo pi in ((MethodInfo) m[0]).GetParameters())
                    {
                        returnString += string.Format("{0} {1}, ", pi.ParameterType, pi.Name);
                    }
                }
                else if (type == "members")
                {
                    MemberInfo[] pa = t.GetMember(propertyname);
                    foreach (var p in pa)
                    {
                        object[] attrs = p.GetCustomAttributes(false);
                        foreach (object o in attrs)
                        {
                            returnString += o.ToString() + ' ';
                        }
                        if (p.MemberType == MemberTypes.Method)
                        {
                            foreach (ParameterInfo pi in ((MethodInfo) p).GetParameters())
                            {
                                returnString += string.Format("{0} {1}, ", pi.ParameterType, pi.Name);
                            }
                        }
                        if (p.MemberType == MemberTypes.Property)
                        {
                            returnString += getPropertyValue(managername, propertyname);
                        }
                    }
                }
                else if (type == "fields")
                {
                    FieldInfo p = t.GetField(propertyname);
                    returnString += p.GetValue(manager);
                }
            }
            catch (Exception e)
            {
                returnString += "ERROR: " + e.Message;
            }
            return returnString;
        }
    }

    public class ThreadingExension : ThreadingExtensionBase
    {

        UdpClient listener;
        string assemblyString;
        WebServiceHost server;

        public void InspectType(Type t)
        {
            assemblyString += t.Name + ":" + Environment.NewLine;
            PropertyInfo[] pia = t.GetProperties();
            foreach (PropertyInfo pi in pia)
            {
                assemblyString += "\t" + pi.PropertyType + " " +  pi.Name + " { get; set; }\n";
            }
            MethodInfo[] mia = t.GetMethods();
            foreach (MethodInfo mi in mia)
            {
                assemblyString += "\t" + mi.ReturnType + " " + mi.Name + "(";
                ParameterInfo[] paramia = mi.GetParameters();
                foreach (ParameterInfo parami in paramia)
                {
                    assemblyString += parami.ParameterType + " "
                        + parami.Name + ",";
                }
                assemblyString += ");\n";
            }
            MemberInfo[] memia = t.GetMembers();
            foreach (MemberInfo memi in memia)
            {
                assemblyString += "\t" + memi.MemberType + " " + memi.Name + ";\n";
            }
        }

        public override void OnCreated(IThreading threading)
        {
            base.OnCreated(threading);

            try
            {
                server = new WebServiceHost(typeof(ManagerService),
                    new Uri("http://localhost:8080/managerservice"));
                server.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }


            try
            {
                NetManager nm = NetManager.instance;
                InspectType(nm.GetType());
                VehicleManager vm = VehicleManager.instance;
                InspectType(vm.GetType());
                CitizenManager cm = CitizenManager.instance;
                InspectType(cm.GetType());
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Error inspecting class: " + e.Message);
            }

            try
            {
                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 11000);
                listener = new UdpClient(ipep);
                listener.Client.ReceiveTimeout = 50;
            }
            catch (Exception e)
            {
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Error creating listener: " + e.Message);
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            listener.Close();
            server.Close();
        }

        public override void OnAfterSimulationTick()
        {
            try
            {
                byte[] data = new byte[1024];

                //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "Receiving!");

                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                data = listener.Receive(ref sender);

                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                    "Got connection from: " + sender.ToString()  + ", message: " +
                    Encoding.ASCII.GetString(data, 0, data.Length));
                
                string welcome = "Welcome to test server:"+Environment.NewLine+ assemblyString;
                data = Encoding.ASCII.GetBytes(welcome);
                listener.Send(data, data.Length, sender);

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                /*
                DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                "Exception: " + e.Message);
                */
            }
        }

    }

} 