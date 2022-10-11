﻿using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class AcreditacionesController : Controller
    {
        readonly ConfigService _Configuracion;
        public AcreditacionesController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpPost("ConseguirAcreditaciones")]
        public IActionResult ConseguirAcreditaciones(string comision, string tipo_acreditacion, [Optional] string categoria_acreditacion, string investigador)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.GetAcreditaciones(_Configuracion, comision, tipo_acreditacion, categoria_acreditacion, investigador);
                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost("Notify")]
        public IActionResult NotifyAcreditaciones(string url, string idUsuario)
        {
            try
            {
                AccionesAcreditaciones accionesAcreditaciones = new AccionesAcreditaciones();
                accionesAcreditaciones.NotifyAcreditaciones(url, idUsuario);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
