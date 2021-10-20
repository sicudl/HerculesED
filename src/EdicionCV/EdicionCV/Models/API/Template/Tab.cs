using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuardadoCV.Models.API.Template
{
    /// <summary>
    /// Modelo de una pesta�a del CV
    /// </summary>
    public class Tab
    {
        /// <summary>
        /// Secciones de la pesta�a del CV
        /// </summary>
        public List<TabSection> sections {get;set;}
    }

    /// <summary>
    /// Secci�n dentro de una pesta�a
    /// </summary>
    public class TabSection
    {
        /// <summary>
        /// T�tulo de la secci�n
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Identificador de la propiedad de la secci�n
        /// </summary>
        public string identifier { get; set; }
        /// <summary>
        /// �rdenes del listado de la secci�n
        /// </summary>
        public List<PresentationOrderTabSection> orders { get; set; }
        /// <summary>
        /// Items del listado de la secci�n
        /// </summary>
        public Dictionary<string,ItemTabSection> items { get; set; }
    }

    /// <summary>
    /// Orden del listado
    /// </summary>
    public class PresentationOrderTabSection
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Propiedades
        /// </summary>
        public List<PresentationOrderTabSectionProperty> properties { get; set; }
    }

    /// <summary>
    /// Propiedades para el orden de la secci�n
    /// </summary>
    public class PresentationOrderTabSectionProperty
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Indica si es ascendente o descendente
        /// </summary>
        public bool asc { get; set; }
    }

    /// <summary>
    /// Item de un listado
    /// </summary>
    public class ItemTabSection
    {
        /// <summary>
        /// T�tulo
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// Indica si es p�blico o no (o no tiene privacidad)
        /// </summary>
        public string ispublic { get; set; }
        /// <summary>
        /// Propiedades del item
        /// </summary>
        public List<ItemTabSectionProperty> properties { get; set; }
        /// <summary>
        /// Propiedades del item para su ordenaci�n
        /// </summary>
        public List<ItemTabSectionOrderProperty> orderProperties { get; set; }
    }

    /// <summary>
    /// Propiedad de un item del listado
    /// </summary>
    public class ItemTabSectionProperty
    {
        /// <summary>
        /// Nombre
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha (sin desplegar)
        /// </summary>
        public bool showMini { get; set; }
        /// <summary>
        /// Indica si se muestra en la minificha en negrita(sin desplegar)
        /// </summary>
        public bool showMiniBold { get; set; }
        /// <summary>
        /// Tipo del dato de la propiedad
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Valores de la propiedad
        /// </summary>
        public List<string> values { get; set; }
    }

    /// <summary>
    /// Propiedad de ordenaci�n de un item del listado
    /// </summary>
    public class ItemTabSectionOrderProperty
    {
        /// <summary>
        /// Propiedad
        /// </summary>
        public string property { get; set; }
        /// <summary>
        /// Valores
        /// </summary>
        public List<string> values { get; set; }
    }

}


