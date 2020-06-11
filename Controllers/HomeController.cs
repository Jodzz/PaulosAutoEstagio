using System;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaulosAutoWithAPI.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Web.Helpers;
using System.IO;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Microsoft.Extensions.Configuration;

namespace PaulosAutoWithAPI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private string URL_API = string.Empty;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            URL_API = configuration["API"]; //Buscar as settings
        }

        // LOGIN
        public void SetCookie(string key, string value, int? expireTime)
        {
            CookieOptions option = new CookieOptions();

            if (expireTime.HasValue)
                option.Expires = DateTime.Now.AddMinutes(expireTime.Value);
            else
                option.Expires = DateTime.Now.AddMilliseconds(10000000);

            Response.Cookies.Append(key, value, option);
        }

        public IActionResult Index(int? result)
        {
            bool getValue = Request.Cookies.TryGetValue("token", out string token);

            if (!getValue)
            {
                if (result.HasValue)
                {
                    ViewBag.Message = result;
                }
                else
                {
                    ViewBag.Message = 0;
                }
                return View();
            }
            else
            {
                return RedirectToAction("EquipamentosHome");
            }
        }

        public IActionResult FazerLogin()
        {
            Uri uri = new Uri(URL_API);
            uri = new Uri(uri, "autenticacao/login");

            string email = Request.Form["email"];
            string password = Request.Form["password"];

            using (var httpClient = new HttpClient())
            {
                StringContent content = null;
                Task<HttpResponseMessage> response = null;
                HttpResponseMessage resultado = null;
                Utilizador user = new Utilizador();
                int postResult = 0;

                content = new StringContent(JsonConvert.SerializeObject(new { email = email, password = password }), Encoding.UTF8, "application/json");

                response = httpClient.PostAsync(uri, content);
                response.Wait();

                resultado = response.Result;

                if (resultado.IsSuccessStatusCode)
                {
                    Task<string> respostaPost = resultado.Content.ReadAsStringAsync();
                    respostaPost.Wait();
                    user = JsonConvert.DeserializeObject<Utilizador>(respostaPost.Result);
                    string roles = String.Join(", ", user.roles);
                    string equipamentos = String.Join(", ", user.equipamentos);

                    SetCookie("token", user.token, null);
                    SetCookie("roles", roles, null);
                    SetCookie("email", user.email, null);

                    return RedirectToAction("EquipamentosHome");
                }
                else
                {
                    postResult = Convert.ToInt32(resultado.StatusCode);
                    return RedirectToAction("Index", new { result = postResult });
                }
            }
        }

        //MUDAR PASSWORD
        public IActionResult MudarPassword(int? result)
        {
            if(result.HasValue)
            {
                ViewBag.Message = result;
            }
            else
            {
                ViewBag.Message = 0;
            }

            return View();
        }

        public async Task<IActionResult> NovaPassword()
        {
            Uri uri = new Uri(URL_API);
            uri = new Uri(uri, "paulosautoapi/utilizador");

            string email = Request.Form["email"];
            string pw = Request.Form["pw"];
            string newpw = Request.Form["newpw"];
            int result = 0;

            using (var httpClient = new HttpClient())
            {
                StringContent content = new StringContent(JsonConvert.SerializeObject(new { email = email, passwordOriginal = pw, passwordNova = newpw }), 
                    Encoding.UTF8, "application/json");

                using (var response = await httpClient.PatchAsync(uri, content))
                {
                    result = Convert.ToInt32(response.StatusCode);
                }
            }

            if (result != 200)
            {
                return RedirectToAction("MudarPassword", new { result = result });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //LOGOUT
        public void RemoveCookie(string key)
        {
            Response.Cookies.Delete(key);
        }

        public IActionResult Logout()
        {
            RemoveCookie("token");
            RemoveCookie("roles");
            RemoveCookie("email");
            return RedirectToAction("Index");
        }

        //HOME PAGE
        public async Task<IActionResult> EquipamentosHome()
        {
            bool getValue = Request.Cookies.TryGetValue("token", out string token);

            if (getValue)
            {
                List<Cliente> listaClientes = new List<Cliente>();
                int nrCliente = 0;

                using (var httpClient = new HttpClient())
                {
                    Uri uri_clientes = new Uri(URL_API);
                    uri_clientes = new Uri(uri_clientes, "paulosautoapi/utilizador/cliente");

                    using (var response = await httpClient.GetAsync(uri_clientes))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listaClientes = JsonConvert.DeserializeObject<List<Cliente>>(apiResponse);
                    }

                    foreach(var cliente in listaClientes)
                    {
                        Request.Cookies.TryGetValue("email", out string email);
                        if (cliente.email == email)
                        {
                            nrCliente = cliente.numeroCliente;
                            SetCookie("nrCliente", Convert.ToString(nrCliente), null);
                        }
                    }
                }

                //cria e recebe lista de elementos da API
                List<Equipamento> listaEquipamentos = new List<Equipamento>();
                using (var httpClient = new HttpClient())
                {
                    Uri uri_equipamentos = new Uri(URL_API);
                    uri_equipamentos = new Uri(uri_equipamentos, "paulosautoapi/clientes/equipamentos/" + nrCliente);

                    using (var response = await httpClient.GetAsync(uri_equipamentos))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listaEquipamentos = JsonConvert.DeserializeObject<List<Equipamento>>(apiResponse);
                    }
                }

                var HomeFilterVM = new HomeFilter
                {
                    listaEquipamentos = listaEquipamentos
                };

                return View(HomeFilterVM);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //EQUIPAMENTOS INDIVIDUAIS
        public async Task<IActionResult> Equipamento(string serialNumber, int? result)
        {
            bool roleHoras = true;
            bool roleHist = true;
            bool getValue = Request.Cookies.TryGetValue("token", out string token);

            Request.Cookies.TryGetValue("roles", out string roles);

            if (!roles.Contains("Administrador"))
            {
                if (!roles.Contains("RegistoHoras"))
                {
                    roleHoras = false;
                }

                if (!roles.Contains("Historico"))
                {
                    roleHist = false;
                }
            }

            if (getValue)
            {
                //cria e recebe lista de elementos da API
                List<Equipamento> listaEquipamentos = new List<Equipamento>();
                using (var httpClient = new HttpClient())
                {
                    Request.Cookies.TryGetValue("nrCliente", out string nrCliente);

                    Uri uri_equipamentos = new Uri(URL_API);
                    uri_equipamentos = new Uri(uri_equipamentos, "paulosautoapi/clientes/equipamentos/" + nrCliente);
                    
                    using (var response = await httpClient.GetAsync(uri_equipamentos))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listaEquipamentos = JsonConvert.DeserializeObject<List<Equipamento>>(apiResponse);
                    }
                }

                List<Avaria> listAvarias = new List<Avaria>();
                using (var httpClient = new HttpClient())
                {
                    Uri uri_avarias = new Uri(URL_API);
                    uri_avarias = new Uri(uri_avarias, "paulosautoapi/equipamentos/avarias/" + serialNumber);

                    using (var response = await httpClient.GetAsync(uri_avarias))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listAvarias = JsonConvert.DeserializeObject<List<Avaria>>(apiResponse);
                    }
                }

                List<Intervenções> listIntervencao = new List<Intervenções>();
                using (var httpClient = new HttpClient())
                {
                    Uri uri_intervencoes = new Uri(URL_API);
                    uri_intervencoes = new Uri(uri_intervencoes, "paulosautoapi/equipamentos/intervencoes/" + serialNumber);

                    using (var response = await httpClient.GetAsync(uri_intervencoes))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listIntervencao = JsonConvert.DeserializeObject<List<Intervenções>>(apiResponse);
                    }
                }

                List<Horas> listHoras = new List<Horas>();
                using (var httpClient = new HttpClient())
                {
                    Uri uri_horas = new Uri(URL_API);
                    uri_horas = new Uri(uri_horas, "paulosautoapi/equipamentos/horas/" + serialNumber);

                    using (var response = await httpClient.GetAsync(uri_horas))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listHoras = JsonConvert.DeserializeObject<List<Horas>>(apiResponse);
                    }
                }

                Equipamento equi = new Equipamento();
                foreach (Equipamento i in listaEquipamentos)
                {
                    if (i.serialNumber == serialNumber) equi = i;
                }

                List<DateTime> listaDatas = new List<DateTime>();
                if (listAvarias.Count != 0)
                {
                    foreach (var item in listAvarias)
                    {
                        if (!listaDatas.Contains(item.dataRegisto))
                        {
                            listaDatas.Add(item.dataRegisto);
                        }
                        else continue;
                    }
                }

                if (listHoras.Count != 0)
                {
                    foreach (var item in listHoras)
                    {
                        if (!listaDatas.Contains(item.key))
                        {
                            listaDatas.Add(item.key);
                        }
                        else continue;
                    }
                }

                if (listIntervencao.Count != 0)
                {
                    foreach (var item in listIntervencao)
                    {
                        foreach (var inter in item.intervencoes)
                        {
                            if (!listaDatas.Contains(inter.data))
                            {
                                listaDatas.Add(inter.data);
                            }
                            else continue;
                        }
                    }
                }

                int resultado = 0;
                if (result.HasValue)
                {
                    if (result == 404 || result == 400)
                    {
                        resultado = Convert.ToInt32(result);
                    }
                }

                var DetalhesEquipVM = new DetalhesEquipamento
                {
                    equipamento = equi,
                    listaAvarias = listAvarias,
                    listaIntervencoes = listIntervencao,
                    listaHoras = listHoras,
                    listaDates = listaDatas,
                    resultadoPost = resultado,
                    acessoHoras = roleHoras, 
                    acessoHist = roleHist
                };

                return View(DetalhesEquipVM);
            }
            else
            {
                string prevURL = Request.Headers["Referer"].ToString();
                return Redirect(prevURL);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnvioHoras()
        {
            string id_equip_horas = Request.Form["id_equip_horas"];
            int equip_horas = Convert.ToInt32(Request.Form["equip_horas"]);
            string id_equip_horas_mobile = Request.Form["id_equip_horas_mobile"];
            int equip_horas_mobile = Convert.ToInt32(Request.Form["equip_horas_mobile"]);

            using (var httpClient = new HttpClient())
            {
                Uri uri_utilizacao = new Uri(URL_API);
                uri_utilizacao = new Uri(uri_utilizacao, "/paulosautoapi/equipamentos/utilizacao");

                StringContent content = null;
                Task<HttpResponseMessage> response = null;
                HttpResponseMessage resultado = null;
                int postResult = 0;
                string serialNumFinal = "";
                Request.Cookies.TryGetValue("email", out string email_cliente);

                if (!String.IsNullOrEmpty(id_equip_horas) && equip_horas != 0)
                {
                    serialNumFinal = id_equip_horas;

                    content = new StringContent(JsonConvert.SerializeObject(new { email = email_cliente, serialNumber = id_equip_horas, horasAtuais = equip_horas }), Encoding.UTF8, "application/json");

                    response = httpClient.PostAsync(uri_utilizacao, content);
                    response.Wait();

                    resultado = response.Result;
                }

                if (!String.IsNullOrEmpty(id_equip_horas_mobile) && equip_horas_mobile != 0)
                {
                    serialNumFinal = id_equip_horas_mobile;

                    content = new StringContent(JsonConvert.SerializeObject(new { email = email_cliente, serialNumber = id_equip_horas_mobile, horasAtuais = equip_horas_mobile }), Encoding.UTF8, "application/json");

                    response = httpClient.PostAsync(uri_utilizacao, content);
                    response.Wait();

                    resultado = response.Result;
                }

                if (resultado.IsSuccessStatusCode)
                {
                    return RedirectToAction("Equipamento", new { serialNumber = serialNumFinal });
                }
                else
                {
                    postResult = Convert.ToInt32(resultado.StatusCode);
                    return RedirectToAction("Equipamento", new { serialNumber = serialNumFinal, result = postResult });
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnvioAvaria()
        {
            Uri uri_avarias = new Uri(URL_API);
            uri_avarias = new Uri(uri_avarias, "paulosautoapi/equipamentos/avarias");

            Request.Cookies.TryGetValue("email", out string email);

            string sNumber = Request.Form["sNumAvaria"];
            string descricao = Request.Form["descAvaria"];

            string sNumberMob = Request.Form["sNumAvariaMobile"];
            string descricaoMob = Request.Form["descAvariaMobile"];

            using (var httpClient = new HttpClient())
            {
                Task<HttpResponseMessage> response = null;
                HttpResponseMessage resultado = null;
                int postResult = 0;
                string serialNumFinal = "";

                if (!String.IsNullOrEmpty(sNumber))
                {
                    serialNumFinal = sNumber;

                    //Os ficheiros estão aqui
                    IFormFileCollection files = Request.Form.Files;

                    var contnetMP = new MultipartFormDataContent("------separadordCampos");
                    contnetMP.Add(new StringContent(email), "Email");
                    contnetMP.Add(new StringContent(sNumber), "SerialNumber");
                    contnetMP.Add(new StringContent(descricao), "Descricao");

                    //agora ficheiros caso tenhas
                    if (files.Count > 0)
                    {
                        foreach (IFormFile ficheiro in files)
                        {
                            if (ficheiro.FileName.Contains(".jpeg") || ficheiro.FileName.Contains(".jpg") || ficheiro.FileName.Contains(".png"))
                            {
                                Stream st = new MemoryStream();
                                ficheiro.CopyTo(st);
                                contnetMP.Add(new StreamContent(st), "Imagens", ficheiro.FileName);
                            }

                            if (ficheiro.FileName.Contains(".mov") || ficheiro.FileName.Contains(".3gp") || ficheiro.FileName.Contains(".mp4"))
                            {
                                Stream st = new MemoryStream();
                                ficheiro.CopyTo(st);
                                contnetMP.Add(new StreamContent(st), "Videos", ficheiro.FileName);
                            }
                        }
                    }

                    response = httpClient.PostAsync(uri_avarias, contnetMP);
                    response.Wait();

                    resultado = response.Result;
                }

                if (!String.IsNullOrEmpty(sNumberMob))
                {
                    serialNumFinal = sNumberMob;

                    //Os ficheiros estão aqui
                    IFormFileCollection files = Request.Form.Files;

                    var contnetMP = new MultipartFormDataContent("------separadordCampos");
                    contnetMP.Add(new StringContent(email), "Email");
                    contnetMP.Add(new StringContent(sNumberMob), "SerialNumber");
                    contnetMP.Add(new StringContent(descricaoMob), "Descricao");

                    //agora ficheiros caso tenhas
                    if (files.Count > 0)
                    {
                        foreach (IFormFile ficheiro in files)
                        {
                            if (ficheiro.FileName.Contains(".jpeg") || ficheiro.FileName.Contains(".jpg") || ficheiro.FileName.Contains(".png"))
                            {
                                Stream st = new MemoryStream();
                                ficheiro.CopyTo(st);
                                contnetMP.Add(new StreamContent(st), "Imagens", ficheiro.FileName);
                            }

                            if (ficheiro.FileName.Contains(".mov") || ficheiro.FileName.Contains(".3gp") || ficheiro.FileName.Contains(".mp4"))
                            {
                                Stream st = new MemoryStream();
                                ficheiro.CopyTo(st);
                                contnetMP.Add(new StreamContent(st), "Videos", ficheiro.FileName);
                            }
                        }
                    }

                    response = httpClient.PostAsync(uri_avarias, contnetMP);
                    response.Wait();

                    resultado = response.Result;
                }

                if (resultado.IsSuccessStatusCode)
                {
                    return RedirectToAction("Equipamento", new { serialNumber = serialNumFinal });
                }
                else
                {
                    postResult = Convert.ToInt32(resultado.StatusCode);
                    return RedirectToAction("Equipamento", new { serialNumber = serialNumFinal, result = postResult });
                }
            }
        }

        //AVARAIS GERAL
        public async Task<IActionResult> Avarias()
        {
            bool getValue = Request.Cookies.TryGetValue("token", out string token);
            Request.Cookies.TryGetValue("roles", out string roles);

            if (!roles.Contains("Administrador"))
            {
                if (!roles.Contains("Historico"))
                {
                    getValue = false;
                }
            }

            if (getValue)
            {
                Request.Cookies.TryGetValue("nrCliente", out string nrCliente);

                Uri uri_equipamentos = new Uri(URL_API);
                uri_equipamentos = new Uri(uri_equipamentos, "paulosautoapi/clientes/equipamentos/" + nrCliente);

                List<Equipamento> listaEquipamentos = new List<Equipamento>();
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(uri_equipamentos))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        listaEquipamentos = JsonConvert.DeserializeObject<List<Equipamento>>(apiResponse);
                    }
                }

                Dictionary<Avaria, string> listaTodasAvarias = new Dictionary<Avaria, string>();
                foreach (var item in listaEquipamentos)
                {
                    List<Avaria> listAvarias = new List<Avaria>();
                    using (var httpClient = new HttpClient())
                    {
                        Uri uri_avarias = new Uri(URL_API);
                        uri_avarias = new Uri(uri_avarias, "paulosautoapi/equipamentos/avarias/" + item.serialNumber);

                        using (var response = await httpClient.GetAsync(uri_avarias))
                        {
                            string apiResponse = await response.Content.ReadAsStringAsync();
                            listAvarias = JsonConvert.DeserializeObject<List<Avaria>>(apiResponse);
                        }
                    }

                    foreach (var avar in listAvarias)
                    {
                        listaTodasAvarias.Add(avar, item.serialNumber);
                    }
                }

                List<DateTime> listaDatas = new List<DateTime>();
                foreach (var item in listaTodasAvarias)
                {
                    if (!listaDatas.Contains(item.Key.dataRegisto))
                    {
                        listaDatas.Add(item.Key.dataRegisto);
                    }
                    else continue;
                }

                listaDatas.Sort((a, b) => b.CompareTo(a));

                var avariasVM = new Avarias
                {
                    listTodasAvarias = listaTodasAvarias,
                    listDates = listaDatas
                };

                return View(avariasVM);
            }
            else
            {
                string prevURL = Request.Headers["Referer"].ToString();
                return Redirect(prevURL);
            }
        }

        //FATURACAO
        public async Task<IActionResult> Faturacao()
        {
            bool getValue = Request.Cookies.TryGetValue("token", out string token);
            Request.Cookies.TryGetValue("roles", out string roles);

            if (!roles.Contains("Administrador")) { 
                if (!roles.Contains("FaturasPendentes"))
                {
                    getValue = false;
                }
            }

            if (getValue)
            {
                Request.Cookies.TryGetValue("nrCliente", out string nrCliente);

                Uri uri_faturacao = new Uri(URL_API);
                uri_faturacao = new Uri(uri_faturacao, "paulosautoapi/clientes/faturas/" + nrCliente);

                List<Fatura> listaFaturas = new List<Fatura>();
                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(uri_faturacao))
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();

                        listaFaturas = JsonConvert.DeserializeObject<List<Fatura>>(apiResponse);
                    }
                }

                return View(listaFaturas);
            }
            else
            {
                string prevURL = Request.Headers["Referer"].ToString();
                return Redirect(prevURL);
            }
        }

        //PÁGINAS DE INFO DA EMPRESA
        public IActionResult Privacy()
        {
            bool testeLogin = Request.Cookies.TryGetValue("roles", out string roles);

            if (testeLogin)
            {
                ViewBag.Message = 1;
            }
            else
            {
                ViewBag.Message = 0;
            }

            return View();
        }

        public IActionResult Cookies()
        {
            bool testeLogin = Request.Cookies.TryGetValue("roles", out string roles);

            if (testeLogin)
            {
                ViewBag.Message = 1;
            }
            else
            {
                ViewBag.Message = 0;
            }

            return View();
        }

        public IActionResult Contactos()
        {
            bool testeLogin = Request.Cookies.TryGetValue("token", out string token);

            if (testeLogin)
            {
                ViewBag.Message = 1;
            }
            else
            {
                ViewBag.Message = 0;
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
