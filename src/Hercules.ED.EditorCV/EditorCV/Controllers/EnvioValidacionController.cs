﻿using EditorCV.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EditorCV.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [EnableCors("_myAllowSpecificOrigins")]
    public class EnvioValidacionController : ControllerBase
    {
        readonly ConfigService _Configuracion;

        public EnvioValidacionController(ConfigService pConfig)
        {
            _Configuracion = pConfig;
        }

        [HttpGet("ObtenerDatosEnvioPRC")]
        public IActionResult ObtenerDatosEnvioPRC(string pIdPersona)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC(_Configuracion);
                return Ok(accionesPRC.ObtenerDatosEnvioPRC(pIdPersona));
            }
            catch (Exception)
            {

            }
            return Ok();
        }

        [HttpPost("EnvioPRC")]
        public IActionResult EnvioPRC([FromForm][Required] string pIdRecurso, [FromForm] string pIdProyecto)
        {
            try
            {
                AccionesEnvioPRC accionesPRC = new AccionesEnvioPRC(_Configuracion);
                accionesPRC.EnvioPRC(_Configuracion, pIdRecurso, pIdProyecto);
            }
            catch (Exception e)
            {
                return Ok(e.Message);
            }

            return Ok();
        }

        [HttpPost("EnvioProyecto")]
        public IActionResult EnvioProyecto([FromForm] string pIdProyecto, [FromForm] string pIdPersona, [FromForm] string pIdAutorizacion)
        {
            try
            {
                AccionesEnvioProyecto accionesProyecto = new AccionesEnvioProyecto();
                accionesProyecto.EnvioProyecto(_Configuracion, pIdProyecto, pIdPersona, pIdAutorizacion);
            }
            catch (Exception)
            {
                throw;
            }

            return Ok();
        }
    }
}
