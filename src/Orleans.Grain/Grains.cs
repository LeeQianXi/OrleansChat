using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrleansTest.IGrains;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace OrleansTest.Grains
{
    public class SessionGrain : Grain, ISessionGrain
    {
        private ConcurrentQueue<UserData> _userdatas= new ConcurrentQueue<UserData>();
        private void Save(bool Empty = false)
        {
            if (!File.Exists(".\\Data\\UserData.json"))
            {
                if (!Directory.Exists(".\\Data\\"))
                {
                    Directory.CreateDirectory(".\\Data\\");
                }
            }
            using (FileStream FS = new FileStream(".\\Data\\UserData.json", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter SW = new StreamWriter(FS))
                {
                    JObject jobj = new JObject();
                    JArray jArray = new JArray();
                    if (!Empty)
                    {
                        foreach (UserData it in _userdatas)
                        {
                            jArray.Add(it.ToJson());
                        }
                    }
                    jobj["UserData"] = jArray;
                    SW.Write(jobj.ToString());
                    SW.Flush();
                }
            }
        }

        private readonly ILogger _logger;
        public SessionGrain(ILogger<ISessionGrain> logger)
        {
            _logger = logger;
            if (!File.Exists(".\\Data\\UserData.json"))
            {
                this.Save();
            }
            try
            {
                using (StreamReader SR = File.OpenText(".\\Data\\UserData.json"))
                {
                    JObject jobj = JObject.Parse(SR.ReadToEnd());
                    foreach (JObject it in (JArray)jobj["UserData"])
                    {
                        _userdatas.Enqueue(new UserData(it));
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex,"");
                this.Save();
                using (StreamReader SR = File.OpenText(".\\Data\\UserData.json"))
                {
                    JObject jobj = JObject.Parse(SR.ReadToEnd());
                    foreach (JObject it in (JArray)jobj["UserData"])
                    {
                        _userdatas.Enqueue(new UserData(it));
                    }
                }
            }
        }

        public ValueTask<int> GetOnlineCount()
        {
            int count = 0;
            foreach (UserData it in _userdatas)
            {
                if (it.IsOnline)
                    count++;
            }
            return ValueTask.FromResult(count);
        }

        public async ValueTask<string> Login(string username, string password)
        {
            _logger.LogInformation($"用户\"{username}\"尝试登录.");
            var ret = await Exist(username);
            if (ret.UserName == string.Empty)
            {
                _logger.LogInformation($"用户名\"{username}\"不存在.");
                return string.Empty;
            }
            else if (!ret.CheckPasswd(password))
            {
                _logger.LogInformation($"用户\"{username}\"密码错误.");
                return string.Empty;
            }
            _logger.LogInformation($"用户\"{username}\"登录成功.");
            ret.IsOnline = true;
            ret.FlushCookie();
            this.Save();
            return ret._cookie;
        }

        public async ValueTask<string> Login(string _cookie)
        {
            Cookie? _old = JsonConvert.DeserializeObject<Cookie>(_cookie);
            if (_old == null)
            {
                _logger.LogInformation($"登录信息为\"null\".");
                return string.Empty;
            }
            _logger.LogInformation($"用户\"{_old.Name}\"尝试登录.");
            var ret = await Exist(_old.Name);
            if (ret.UserName == string.Empty)
            {
                _logger.LogInformation($"用户名\"{_old.Name}\"不存在.");
                return string.Empty;
            }
            Cookie Cret = JsonConvert.DeserializeObject<Cookie>(ret._cookie);
            if (Cret.Value == _old.Value)
            {
                if (Cret.Expires > DateTime.UtcNow)
                {
                    _logger.LogInformation($"用户\"{_old.Name}\"登录成功.");
                    ret.FlushCookie();
                    ret.IsOnline = true;
                    Save();
                    return ret._cookie;
                }
                else
                {
                    _logger.LogInformation($"用户\"{_old.Name}\"登录过期.");
                    return _cookie;
                }
            }
            else
            {
                return string.Empty;
            }
        }

        public async Task Logout(string username, string cookie)
        {
            var ret = await Exist(username);
            if (ret.UserName == string.Empty)
            {
                _logger.LogInformation($"用户名\"{username}\"不存在.");
            }
            else if (ret.IsOnline)
            {
                ret.IsOnline=false;
                _logger.LogInformation($"用户\"{username}\"已登出.");
            }
            else
            {
                _logger.LogInformation($"尝试登出未登录的用户\"{username}\".");
            }
        }

        public ValueTask<UserData> Exist(string username)
        {
            lock (_userdatas)
            {
                foreach (UserData it in _userdatas)
                {
                    if (it.UserName == username)
                    {
                        _logger.LogInformation($"用户库中存在用户{username}.");
                        return ValueTask.FromResult(it);
                    }
                }
            }
            _logger.LogInformation($"用户库中不存在用户{username}.");
            return ValueTask.FromResult(UserData.Empty);
        }

        public async Task Register(string username, string password)
        {
            if ((await Exist(username)).UserName==string.Empty)
            {
                var user = new UserData(username, password);
                _userdatas.Enqueue(user);
                _logger.LogInformation($"成功注册用户\"{user.UserName}\".");
                this.Save();
            }
            _logger.LogInformation($"用户名\"{username}\"已被使用.");
        }
    }
    public class UserData
    {
        private string _username;
        public string UserName
        {
            get
            {
                return _username;
            }
            private set
            {
                _username = value;
            }
        }
        private byte[] _password;
        public byte[] Password
        {
            get
            {
                return _password;
            }
            private set
            {
                _password = value;
            }
        }
        private bool _online;
        public bool IsOnline
        {
            get
            {
                return _online;
            }
            internal set
            {
                _online = value;
            }
        }
        internal string _cookie;
        public string Cookie
        {
            get
            {
                return _cookie;
            }
            internal set
            {
                _cookie = value;
            }
        }
        public UserData()
        {
            _username = string.Empty;
            _password = [];
            _cookie = string.Empty;
        }
        public UserData(string username, string password)
        {
            _username = username;
            _password = MD5.HashData(Encoding.UTF8.GetBytes(password));
            _cookie = JsonConvert.SerializeObject(
                new Cookie(username, new Guid(_password).ToString())
                {
                    Expires= DateTime.UtcNow.AddDays(1),
                }
                );
        }
        public UserData(JObject json)
        {
            _username = (string)json["UserName"];
            _password = StringToBytes((string)json["Password"]);
            _cookie = (string)json["Cookie"];
        }

        public bool CheckPasswd(string password)
        {
            return _password.SequenceEqual(MD5.HashData(Encoding.UTF8.GetBytes(password)));
        }
        public string FlushCookie()
        {
            this._cookie = JsonConvert.SerializeObject(
                new Cookie(_username, new Guid(_password).ToString())
                {
                    Expires = DateTime.UtcNow.AddDays(1)
                }
                );
            return this._cookie;
        }

        public static explicit operator JObject(UserData data)
        {
            JObject obj = new JObject();
            obj["UserName"] = data._username;
            obj["Password"] = BytesToString(data._password);
            obj["Cookie"] = data._cookie;
            return obj;
        }
        public JObject ToJson()
        {
            return (JObject)this;
        }

        public static UserData Empty
        {
            get
            {
                return new UserData();
            }
        }

        public static string BytesToString(byte[] bytes)
        {
            string buffer=string.Empty;
            foreach (byte b in bytes)
            {
                buffer += b.ToString("X2");
            }
            return buffer;
        }
        public static byte[] StringToBytes(string str)
        {
            if (str.Length % 2 != 0)
            {
                throw new ArgumentException("不合法的输入");
            }
            byte[] buffer = new byte[str.Length / 2];
            for(int i = 0; i < str.Length/2; i ++)
            {
                buffer[i] = byte.Parse(str.Substring(2 * i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return buffer;
        }
    }
}