﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using System.Web.Script.Serialization;

using ICities;
using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Plugins;

using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Channels;

namespace NetworkAPI
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class Network : INetwork
    {
        JavaScriptSerializer serializer;

        public Network()
        {
            serializer = new JavaScriptSerializer();
        }

        public List<string> GetAssemblyTypes(string assemblyName)
        {
            List<string> types = new List<string>();
            types.Add("GetAssemblyTypes:: ");
            Assembly assembly = Assembly.Load(assemblyName);
            types = assembly.GetTypes().Select(x => x.Name).ToList<string>();
            return types;
        }

        public List<string> GetManagers()
        {
            List<string> managers = new List<string>();
            managers.Add("GetManagers:: ");
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
            properties.Add("GetManagerProperties:: ");
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
            string returnString = "GetManagerProperty:: ";
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
                else if (type == "events")
                {

                }
                else if (type == "nestedTypes")
                {
                    //Type nt = t.GetNestedType(propertyname);
                    Citizen testObj = new Citizen();
                    returnString += serializer.Serialize(testObj);
                }
            }
            catch (Exception e)
            {
                returnString += "ERROR: " + e.Message;
            }
            return returnString;
        }

        public string CallManagerMethod(string managername, string methodname, string paramdata)
        {
            DebugOutputPanel.AddMessage(PluginManager.MessageType.Message,
                "GET call");
            Console.WriteLine("Received GET for CallManagerMethod: " + paramdata);

            string retString = "";
            
            if (paramdata != null && paramdata.Length > 0)
            {
                IDictionary<string, object> dict = serializer.DeserializeObject(paramdata) as IDictionary<string, object>;
                if (dict != null)
                {
                    retString = serializer.Serialize(dict);
                    Console.WriteLine("Dict: " + retString);
                }
            }

            return retString;
        }
    }
}
