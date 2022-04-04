const uriLoadTaxonomies = "Cluster/GetThesaurus"
const uriSaveCluster = "Cluster/SaveCluster"
const uriSearchTags = "Cluster/searchTags"
const uriLoadProfiles = "Cluster/loadProfiles"

var servicioExternoBaseUrl="";

var urlLT = "";
var urlSC ="";
var urlSTAGS = "";
var urlCargarPerfiles = "";
var stepsCls;

$(document).ready(function () {
	servicioExternoBaseUrl=$('#inpt_baseURLContent').val()+'/servicioexterno/';
	urlLT = new URL(servicioExternoBaseUrl +  uriLoadTaxonomies);
	urlSC = new URL(servicioExternoBaseUrl +  uriSaveCluster);
	urlSTAGS = new URL(servicioExternoBaseUrl +  uriSearchTags);
	urlCargarPerfiles = new URL(servicioExternoBaseUrl +  uriLoadProfiles);
	stepsCls = new StepsCluster();
});




class StepsCluster {
	/**
	 * Constructor de la clase StepsCluster
	 */
	constructor() {
		var _self = this
		this.step = 1
		this.body = $('body')

		// Secciones principales
		this.modalCrearCluster = this.body.find('#modal-crear-cluster')
		this.stepProgressWrap = this.modalCrearCluster.find(".step-progress-wrap")
		this.stepsCircle = this.stepProgressWrap.find(".step-progress__circle")
		this.stepsBar = this.stepProgressWrap.find(".step-progress__bar")
		this.stepsText = this.stepProgressWrap.find(".step-progress__text")
		this.modalCrearClusterStep1 = this.modalCrearCluster.find("#modal-crear-cluster-step1")
		this.modalCrearClusterStep2 = this.modalCrearCluster.find("#modal-crear-cluster-step2")
		this.modalCrearClusterStep3 = this.modalCrearCluster.find("#modal-crear-cluster-step3")
		this.clusterAccordionPerfil = this.modalCrearCluster.find("#accordion_cluster")
		this.errorDiv = this.modalCrearCluster.find("#error-modal-cluster")
		this.errorDivServer = this.modalCrearCluster.find("#error-modal-server-cluster")
		this.perfilesStep3 = this.modalCrearClusterStep3.find("#perfiles-stp3-result-cluster")

		this.stepContentWrap = this.modalCrearCluster.find(".steps-content-wrap")
		this.stepsContent = this.stepContentWrap.find(".section-steps")

		// Añadir perfil
		this.modalPerfil = this.body.find("#modal-anadir-perfil-cluster")
		this.inputPerfil = this.modalPerfil.find("#input-anadir-perfil")

		// Areas temáticas Modal
		this.modalAreasTematicas = this.body.find('#modal-seleccionar-area-tematica')
		this.divTesArbol = this.modalAreasTematicas.find('.divTesArbol')
		this.divTesLista = this.modalAreasTematicas.find('.divTesLista')
		this.divTesListaCaths = undefined
		this.btnSaveAT = this.modalAreasTematicas.find('.btnsave')

		// Información para el guardado 
		this.userId = document.getElementById('inpt_usuarioID').value
		this.clusterId = undefined
		this.communityShortName = this.modalCrearCluster.data('cshortName')
		this.communityUrl = this.modalCrearCluster.data('comUrl')
		this.communityKey = this.modalCrearCluster.data('comKey')

		// Tags
		this.topicsM = undefined

		// Textos obtenido de los 'data-'
		this.eliminarText = this.modalCrearCluster.data("eliminartext")
		this.areasTematicasText = this.modalCrearCluster.data("areastematicastext")
		this.descriptoresEspecificosText = this.modalCrearCluster.data("descriptoresespecificostext")

		// Incia las funcionalidades iniciales de modal si este se abre
		this.modalCrearCluster.on('shown.bs.modal', function () {
			_self.init()
		});

	}

	/**
	 * Método que inicia el funcionamiento funcionalidades necesarias para el creador de clusters
	 */
	init() {
		var _self = this
		// Fill taxonomies data
		this.getDataTaxonomies().then((data) => {
			_self.fillDataTaxonomies(data);
		})

		this.topicsM = new ModalSearchTags()
	}
	
	/**
	 * Método que inicia las comprobaciones para pasar a la siguiente sección
	 * param pos: Posición a la que se quiere pasar
	 */
	async goStep(pos) {
		if (pos > 0 && this.step > pos) {

			this.errorDiv.hide()
			this.errorDivServer.hide()
			this.setStep(pos)

		} else if(pos > 0) {

			let continueStep = true
			switch (this.step) {
				case 1:
				continueStep = this.checkContinue1()
				break;
				case 2:
				continueStep = this.checkContinue2()
				if (continueStep) {
					try {
						continueStep = await this.saveInit()
					} catch(err) {
						// this.errorDiv.show()
						this.errorDivServer.show()
						window.location.hash = '#' + this.errorDivServer.attr('id')
						continueStep = false;
					}
				}
				break;
			}

			if (continueStep && this.step > (pos - 2)) {
				this.errorDiv.hide()
				this.errorDivServer.hide()
				this.setStep(pos)
			} else {
				this.errorDiv.show()
				window.location.hash = '#' + this.errorDiv.attr('id')
			}
		}
	}

	/**
	 * Método que borra un perfil
	 */
	deletePerfil(head1, head2) {
		$('#' + head1).remove()
		$('#' + head2).remove()
		// $(item).parent().parent().remove()
	}

	/**
	 * Método que comprueba que los campos obligatorios de la sección 1 han sido rellenados
	 * También guarda el estado de la sección 1
	 * return bool: Devuelve true or false dependiendo de si ha pasado la validación
	 */
	checkContinue1() {
		var _self = this

		// Get first screen data
		let name = document.getElementById('nombreclusterinput').value
		let description = document.getElementById('txtDescripcion').value
		let terms = []
		let inputsTermsItms = this.modalCrearClusterStep1.find('#cluster-modal-sec1-tax-wrapper').find('input')
		inputsTermsItms.each((i, e) => {terms.push(e.value)})

		this.data = {
			entityID: _self.clusterId,
			name,
			description,
			terms
		}

		return (name.length > 0 && terms.length > 0)
	}

	/**
	 * Método que comprueba que al menos hay un perfil con areas temáticas para la sección 2
	 * También guarda el estado de la sección 2
	 * return bool: Devuelve true or false dependiendo de si ha pasado la validación
	 */
	checkContinue2() {
		var _self = this

		// Get the second screen
		let profiles = this.modalCrearClusterStep2.find('.panel-collapse')
		let profilesObjets = []

		profiles.each((i, e) => {
			let termsSec = $(e).find('.terms-items')
			let inputsTermsProf = termsSec.find('input')
			let profTerms = []
			inputsTermsProf.each((i, e) => {profTerms.push(e.value)})


			let topicsSec = $(e).find('.tags-items')
			let inputsTopicsProf = topicsSec.find('input')
			let profTags = []
			inputsTopicsProf.each((i, e) => {profTags.push(e.value)})

			profilesObjets.push({
				"name": $(e).data('name'),
				"terms": profTerms,
				"tags": profTags,
			})
		})

		// Set the post data
		this.data = {
			...this.data,
			profiles: profilesObjets
		}

		return (this.data.profiles.length > 0 && this.data.profiles[0].name != undefined && this.data.profiles[0].name.length > 0 && this.data.profiles[0].terms.length > 0)
	}

	/**
	 * Método que genera la petición get para obtener las taxonomías
	 */
	getDataTaxonomies() {
		
		// https://localhost:44321/Cluster/GetThesaurus?listadoCluster=%5B%22researcharea%22%5D
		let listThesaurus = ["researcharea"];
		urlLT.searchParams.set('listThesaurus', JSON.stringify(listThesaurus));

		return new Promise((resolve, reject) => {
			$.get(urlLT.toString(), function (data) {
				resolve(data)
			});
		})
	}

	/**
	 * Inicia la generación del html para las diferentes taxonomías
	 * param data, Objeto con los items
	 */
	fillDataTaxonomies(data) {
		// Set tree
		let resultHtml = this.fillTaxonomiesTree(data['researcharea'])
		this.divTesArbol.append(resultHtml)

		// Set list
		/* resultHtml = this.fillTaxonomiesList(data['researcharea'])
		this.divTesLista.append(resultHtml)
		this.divTesListaCaths = this.divTesLista.find(".categoria-wrap") */

		// Open & close event trigger
		let desplegables = this.modalAreasTematicas.find('.boton-desplegar')
	
		if (desplegables.length > 0) {
			desplegables.off('click').on('click', function () {
				$(this).toggleClass('mostrar-hijos');
			});
		}

		// Add events when the items are clicked
		this.itemsClicked();
	}

	/**
	 * Add events when the Taxonomies items are clicked
	 */
	itemsClicked() {

		var _self = this

		// Click into the tree
		this.divTesArbol.off('click').on("click", "input.at-input", function() {
			let dataVal = this.checked
			let dataId = $(this).attr('id')
			let dataParentId = $(this).data('parentid')
			dataParentId = (dataParentId.length > 0) ? dataParentId.split('/').pop() : dataParentId

			if (dataParentId.length > 0) {
				if (!dataVal) {
					let brothers = $(this).parent().parent().parent().parent().find('input.at-input:checked')
					if (brothers.length == 0) {
						_self.selectParent(dataParentId, false)
					} else {
						_self.selectParent(dataParentId)
					}
				} else {
					_self.selectParent(dataParentId)
				}
			}

			
		})

		// Click into the list
		/* this.divTesLista.off('click').on("click","input.at-input", function() {
			let dataVal = this.checked
			let dataId = $(this).attr('id').substring("list__".length)

			if (dataVal) {
				document.getElementById(dataId).checked = true
			} else  {
				document.getElementById(dataId).checked = false
			}
		}) */
	}

	/**
	 * Select the parent in the list and add a class to active the item
	 * param pId, the parent id
	 * param addclass, check if should dishabled the item
	 */
	selectParent(pId, addclass = true) {

		let itemP = document.getElementById(pId)
		let $itemP = $(itemP)
		// Check if add or remove class select
		if (addclass) {
			$itemP.addClass('selected')
		} else {
			// Search for childs and remove selected class if not has childs enhabled
			let childs = $itemP.parent().parent().parent().find('input.at-input:checked')
			if (childs.length == 0) {
				$itemP.removeClass('selected')
			} else {
				// Stop the recursive function if it's childs checked
				return null
			}
		}
		// Call to the parent to change the 'selected' class
		let dataParentId = $itemP.data('parentid')
		dataParentId = (dataParentId.length > 0) ? dataParentId.split('/').pop() : dataParentId

		if (dataParentId.length > 0) {this.selectParent(dataParentId, addclass)}
	}

	/**
	 * Crea el html con las taxonomías
	 * param data, array con los items
	 * param idParent, id del nodo padre, para generar los hijos
	 * return string con el texto generado
	 */
	fillTaxonomiesTree(data, idParent = "") {

		var _self = this;

		let resultHtml = "";
		data.filter(e => e.parentId == idParent).forEach(e => {
			let id = e.id.split('/').pop()
			
			let children = _self.fillTaxonomiesTree(data, e.id);

			let disabled = (children != "" && children != undefined) ? 'disabled="disabled"' : ""
			
			resultHtml += '<div class="categoria-wrap">\
					<div class="categoria ' + id + '">\
						<div class="custom-control custom-checkbox themed little primary">\
							<input type="checkbox" class="custom-control-input at-input" id="' + id + '" data-id="' + e.id + '" data-name="' + e.name + '" data-parentid="' + e.parentId + '" ' + disabled +'>\
							<label class="custom-control-label" for="' + id + '">' + e.name + '</label>\
						</div>\
					</div>'


			if (children != "" && children != undefined) {
				resultHtml += '<!--  pintar esto solo cuando tenga hijos -->\
					<div class="boton-desplegar">\
						<span class="material-icons">keyboard_arrow_down</span>\
					</div> \
					<!--  -->'

				resultHtml += '<div class="panHijos">' + children + '</div>'
			}

			resultHtml += '</div>'
		});

		return resultHtml
	}

	/**
	 * Crea el html con las taxonomías en arbol
	 * param data, array con los items
	 * return string con el texto generado
	 */
	fillTaxonomiesList(data) {

		var _self = this;

		let resultHtml = "";
		data.forEach(e => {
			let id = e.id.split('/').pop()
			resultHtml += '<div class="categoria-wrap" data-text="' + e.name + '">\
					<div class="categoria list__' + id + '">\
						<div class="custom-control custom-checkbox themed little primary">\
							<input type="checkbox" class="custom-control-input at-input" id="list__' + id + '" data-id="' + e.id + '" data-parentid="' + e.parentId + '" data-name="' + e.name + '">\
							<label class="custom-control-label" for="list__' + id + '">' + e.name + '</label>\
						</div>\
					</div>\
				</div>'
		});

		return resultHtml
	}

	/**
	 * Filtra los items de la lista de categorías
	 * param input con el texto a filtrar
	 */
	MVCFiltrarListaSelCat(item) {

		// Get the text
		let searchTxt = $(item).val()

		// Set the RegExp
		let matcher = new RegExp(searchTxt, "i");

		// Search the text into the items
		let notFounds = this.divTesListaCaths.each((i, e) => {
			if ($(e).data("text") != null && $(e).data("text") != undefined && $(e).data("text").search(matcher) != -1) {
				$(e).removeClass('d-none');
			} else if($(e).data("text") != null && $(e).data("text") != undefined) {
				$(e).addClass('d-none');
			}
		})

	}

	/**
	 * Método que guarda los 2 pasos iniciales
	 */
	saveInit() {

		var _self = this

		// Set the url parameters
		urlSC.searchParams.set('pIdGnossUser', this.userId)
		// urlSC.searchParams.set('pIdGnossComSName', this.communityShortName)
		return new Promise((resolve) => {

			this.postCall(urlSC, this.data).then((data) => {
				_self.clusterId = data
				console.log("_self.clusterId", _self.clusterId)
				// _self.goStep(3)
				_self.startStep3()
				resolve(true)
			}).catch(err => {
				resolve(false)
			}) 
		})

	}

	/** 
	 * Método que realiza la llamada POST
	 * param url, objeto URL que contiene la url de la petición POST
	 * param theParams, parámetros para la petición POST
	 */
	postCall(url, theParams) {
		let _self = this
		return new Promise((resolve) => {
			$.post(url.toString(), theParams, function(data) {
				resolve(data)
			}).fail(function(err) {
				resolve(err);
			})
		})
	}

	/**
	 * Carga todas las áreas temáticas seleccionadas para ese perfil / sección 
	 * param item, sección donde se encuentra la información para cargar las areas temáticas
	 */
	setAreasTematicas(item) {
		let _self = this

		let relItem = $('#' + $(item).data("rel"))

		if (relItem.length > 0) {

			let dataJson = relItem.data('jsondata')

			// Reestablecer las categorías
			this.divTesArbol.find('input.at-input').each((i, e) => {
				e.checked = false
				e.classList.remove('selected')
			})

			// Establece las categorías de la sección
			if (typeof dataJson == "object") {

				dataJson.forEach(e => {
					let item = document.getElementById(e)
					item.checked = true

					// Comprueba si tiene padre para establecerlo con habilitado.
					let parentId = item.getAttribute('data-parentid')
					if (parentId.length > 0) {
						parentId = (parentId.length > 0) ? parentId.split('/').pop() : parentId

						_self.selectParent(parentId)
					}
				})
			}


			// Muestra el modal de las áreas temáticas
			this.modalAreasTematicas.modal('show')

			this.saveAreasTematicas(relItem)
		}
	}

	/**
	 * Método que inicia el modal de los tópicos 
	 */
	loadModalTopics(el) {

		let parent = $(el).parent()

		// Establecer los tags de la sección
		this.setTAGS(el)
		// Inicia el funcionamiento de los tags
		this.topicsM.init()

		this.topicsM.closeBtnClick().then((data) => {
			// parent.data('jsondata', data)
			let relItem = $('#' + $(el).data("rel"))
			console.log("///// closeBtnClick CLICKADO /////")
			console.log('$(el).data("rel")', $(el).data("rel"))
			console.log("data", data)
			this.saveTAGS(relItem, data)
		})
	}

	/**
	 * Método que genera el evento para añadir los tags selecciondas en el popup
	 * param relItem, elemento relacionado para indicar dónde deben de guardarse las areas temátcias seleccionadas
	 */
	saveTAGS(relItem, data) {

		console.log("relItem", relItem)
		console.log("data", data)
		console.log("relItem.data()", relItem.data())

		let htmlResWrapper = $('<div class="tag-list mb-4 d-inline"></div>')

		let htmlRes = ''
		let arrayItems = [] // Array para guardar los items que se van a usar
		data.forEach(e => {
			htmlRes += `<div class="tag" title="` + e + `" data-id="` + e + `">
				<div class="tag-wrap">
					<span class="tag-text">` + e + `</span>
					<span class="tag-remove material-icons">close</span>
				</div>
				<input type="hidden" value="` + e + `">
			</div>`
		})

		htmlResWrapper.append(htmlRes)

		relItem.html(htmlResWrapper)

		// Se añade un json para saber qué categorías se han seleccionado
		relItem.data('jsondata', data)

		this.deleteTAGS(relItem)
	}



	/**
	 * Método que elimina las etiquetas del arbol y del listado
	 * param relItem, contenedor donde se encuentran las áreas temáticas seleccionadas
	 */
	deleteTAGS(relItem) {

		// Selecciona la áreas temáticas seleccionadas dentro de selector
		let tagsItems = relItem.find('.tag-wrap');
		tagsItems.on('click', '.tag-remove', function() {
			// Selecciona el item padre para eliminar
			let itemToDel = $(this).parent().parent()
			let inputShortId = itemToDel.data('id')

			// Remove the current item from array
			let jsonItems = relItem.data('jsondata')
			let myIndex = jsonItems.indexOf(inputShortId)
			if (myIndex !== -1) {
				jsonItems.splice(myIndex, 1)
			}


			// Delete item
			itemToDel.remove()

		})
	}


	/**
	 * Carga todas las áreas temáticas seleccionadas para ese perfil / sección 
	 * param item, sección donde se encuentra la información para cargar las areas temáticas
	 */
	setTAGS(item) {
		let _self = this

		let relItem = $('#' + $(item).data("rel"))

		if (relItem.length > 0) {

			let dataJson = relItem.data('jsondata')

			// Reestablecer las categorías
			_self.topicsM.removeTags()

			// Establece las categorías de la sección
			if (typeof dataJson == "object") {

				dataJson.forEach(e => {
					_self.topicsM.addTag(e)
				})
			}
		}
	}


	/**
	 * Método que genera el evento para añadir las áreas temáticas selecciondas en el popup
	 * param relItem, elemento relacionado para indicar dónde deben de guardarse las areas temátcias seleccionadas
	 */
	saveAreasTematicas(relItem) {

		// Evento para cuando se seleccione guardar las áreas temáticas
		this.btnSaveAT.off('click').on('click', (e) => {
			e.preventDefault()

			// Oculta el modal de las áreas temáticas
			this.modalAreasTematicas.modal('hide')

			// Selecciona y establece el contenedor de las areas temáticas
			// let relItem = $('#' + $(item).data("rel"))

			let htmlResWrapper = $('<div class="tag-list mb-4 d-inline"></div>')

			let htmlRes = ''
			let arrayItems = [] // Array para guardar los items que se van a usar
			this.divTesArbol.find('input.at-input').each((i, e) => {
				if (e.checked) {
					htmlRes += '<div class="tag" title="' + $(e).data('name') + '" data-id="' + $(e).data('id') + '">\
						<div class="tag-wrap">\
							<span class="tag-text">' + $(e).data('name') + '</span>\
							<span class="tag-remove material-icons">close</span>\
						</div>\
						<input type="hidden" value="' + $(e).data('id') + '">\
					</div>'
					arrayItems.push(e.id)
				}
			})

			htmlResWrapper.append(htmlRes)

			relItem.html(htmlResWrapper)

			// Se añade un json para saber qué categorías se han seleccionado
			relItem.data('jsondata', arrayItems)

			this.deleteAreasTematicas(relItem)

		})
	}


	/**
	 * Método que elimina las áreas temáticas del arbol y del listado
	 * param relItem, contenedor donde se encuentran las áreas temáticas seleccionadas
	 */
	deleteAreasTematicas(relItem) {

		// Selecciona la áreas temáticas seleccionadas dentro de selector
		let tagsItems = relItem.find('.tag-wrap');
		tagsItems.on('click', '.tag-remove', function() {
			// Selecciona el item padre para eliminar
			let itemToDel = $(this).parent().parent()
			let inputShortId = itemToDel.data('id')

			if (inputShortId.length > 0) {
				inputShortId = inputShortId.split('/').pop()
			}

			// Set the inputs into the areas temáticas in false
			try {
				document.getElementById(inputShortId).checked = false
				// document.getElementById('list__' + inputShortId).checked = false
			}catch (error) { }

			// Remove the current item from array
			let jsonItems = relItem.data('jsondata')
			let myIndex = jsonItems.indexOf(inputShortId)
			if (myIndex !== -1) {
				jsonItems.splice(myIndex, 1)
			}


			// Delete item
			itemToDel.remove()

		})
	}

	/**
	 * Método que añade un perfil nuevo en el segundo paso
	 */
	addPerfilSearch() {

		// Get the name
		let name = this.inputPerfil.val()
		this.modalPerfil.modal('hide')
		this.inputPerfil.val("") // Set the name empty

		// Get the image user url
		let imgUser = this.modalCrearCluster.data('imguser')

		// Set The item id
		let nameId = name.replace(/[^a-z0-9_]+/gi, '-').replace(/^-|-$/g, '').toLowerCase()
		let rand = Math.random() * (100000 - 10000) + 10000
		nameId = nameId + rand.toFixed()

		// Get the panel item
		let panel = this.clusterAccordionPerfil.find('.panel')

		// 
		let item = `<div class="panel-heading" role="tab" id="`+ nameId +`-tab">
				<p class="panel-title">
					<a class="perfil" data-toggle="collapse" data-parent="#accordion_cluster" href="#`+ nameId +`" aria-expanded="true" aria-controls="`+ nameId +`" data-expandable="false">
						<span class="material-icons">keyboard_arrow_down</span>
						<img src="`+ imgUser +`" alt="person">
						<span class="texto">`+ name +`</span>
					</a>
				</p>
				<a href="javascript:void(0)" onclick="stepsCls.deletePerfil('`+ nameId +`-tab', '`+ nameId +`')" class="btn btn-outline-grey eliminar">
					` + this.eliminarText + `
					<span class="material-icons-outlined">delete</span>
				</a>
			</div>
			<div id="`+ nameId +`" data-name="`+ name +`" class="panel-collapse collapse show" role="tabpanel" aria-labelledby="`+ nameId +`-tab">
				<div class="panel-body">

					<!-- Areas temáticas -->
					<div class="form-group mb-5 edit-etiquetas terms-items">
						<label class="control-label d-block">`+ this.areasTematicasText +`</label>
						<div class="autocompletar autocompletar-tags form-group" id="modal-seleccionar-area-tematica-`+ nameId +`">
							<div class="tag-list mb-4 d-inline"></div> 
						</div>
						<a class="btn btn-outline-primary" href="javascript: void(0)">
							<span class="material-icons" onclick="stepsCls.setAreasTematicas(this)" data-rel="modal-seleccionar-area-tematica-`+ nameId +`">add</span>
						</a>
					</div>
					<!-- -->

					<!-- Tópicos -->
					<div class="form-group mb-5 edit-etiquetas tags-items">
						<label class="control-label d-block">` + this.descriptoresEspecificosText + `</label>
						<div class="autocompletar autocompletar-tags form-group" id="modal-seleccionar-tags-`+ nameId +`">
							<div class="tag-list mb-4 d-inline"></div> 
						</div>
						<a class="btn btn-outline-primary" href="javascript: void(0)">
							<span class="material-icons" data-rel="modal-seleccionar-tags-`+ nameId +`" onclick="stepsCls.loadModalTopics(this)">add</span>
						</a>
					</div>
					<!-- -->

				</div>
			</div>`

		panel.append(item)
	}


	/**
	 * Establece el "estado" del "step-progress"
	 * param tstep, posición a establecer
	 */
	setStep(tstep) {
		this.step = tstep

		// Steps Checks
		this.stepsCircle.each((index, step) => {
			$(step).removeClass("done active")
		})

		// Bar step
		this.stepsBar.each((index, step) => {
			$(step).removeClass("active")
		})

		// Title step
		this.stepsText.each((index, step) => {
			$(step).removeClass("current")
		})

		// Content
		this.stepsContent.each((index, step) => {
			$(step).removeClass("show")
		})

		// Add class before
		for (var i = 0; i < (this.step - 1); i++) {
			$(this.stepsCircle[i]).addClass("done active")
			$(this.stepsBar[i]).addClass("active")

		}
		// Add class current
		$(this.stepsCircle[this.step - 1]).addClass("active")
		$(this.stepsContent[this.step - 1]).addClass("show")
		$(this.stepsText[this.step - 1]).addClass("current")

	}

	startStep3() { 
		comportamientoPopupCluster.init(this.data)
		this.PrintPerfilesstp3 ()
	}

	/**
	 * Pintar los perfiles "finales"
	 */
	PrintPerfilesstp3 () {

		let imgUser = this.modalCrearCluster.data('imguser')

		let profiles = this.data.profiles.map((e, i) => {
			let idAccordion = (e.name.replace(/[^a-z0-9_]+/gi, '-').replace(/^-|-$/g, '').toLowerCase() + "-" + i)
			return $(`<div class="panel-group pmd-accordion" id="accordion-`+ idAccordion +`" role="tablist" aria-multiselectable="true">
				<div class="panel">
					<div class="panel-heading" role="tab" id="experto-`+ idAccordion +`-tab">
						<p class="panel-title">
							<a class="perfil" data-toggle="collapse" data-parent="#accordion" href="#experto-`+ idAccordion +`" aria-expanded="true" aria-controls="experto-1" data-expandable="false">
								<span class="material-icons">keyboard_arrow_down</span>
								<img src="`+ imgUser +`" alt="imguser">
								<span class="texto">`+ e.name +`</span>
							</a>
						</p>
					</div>
					<div id="experto-`+ idAccordion +`" class="panel-collapse collapse show" role="tabpanel" aria-labelledby="experto-`+ idAccordion +`-tab">
						<div class="panel-body">

							<table class="display nowrap table table-sm tableusers">
								<thead>
									<tr>
										<th class="th-user">

										</th>
										<th class="th-publicaciones">
											Publicaciones
										</th>
										<th class="th-principal">
											Principal
										</th>
										<th class="th-acciones">

										</th>
									</tr>
								</thead>
								<tbody>
									
								</tbody>
							</table>
						</div>
					</div>
				</div>
			</div>`)
		})

		this.perfilesStep3.append(profiles)
	}

	/** 
	* Añade un usuario perfil de las pestañas y al objeto
	*/
	addUserClusterStep3 (itemToAdd) {
		let user = {}
		// Obtiene el usuario 
		// TODO

		var userHTML = $(`
			<tr>
				<td class="td-user">
					<div class="user-miniatura">
						<div class="imagen-usuario-wrap d-none">
							<a href="javascript:void(0)">
								<div class="imagen">
									<span style="background-image: url(theme/resources/imagenes-pre/foto-usuario-2.jpg)"></span>
								</div>
							</a>
						</div>
						<div class="nombre-usuario-wrap">
							<a href="`+ user.url +`">
								<p class="nombre">`+ user.name +`</p>
								<p class="nombre-completo">`+ user.category +`. `+ user.departament +`</p>
							</a>
						</div>
					</div>
				</td>
				<td class="td-publicaciones">
					`+ user.publicacions +`
				</td>
				<td class="td-principal">
					`+ user.principal +`
				</td>
				<td class="td-acciones">
					<ul class="no-list-style">
						<li>
							<a href="javascript: void(0);" class="texto-gris-claro">
								Eliminar
								<span class="material-icons-outlined">delete</span>
							</a>
						</li>
					</ul>
				</td>
			</tr>
		`)

		$('accordion-' + itemToAdd).find('table.tableusers tbody').append(userHTML)
	}

}


// Clase para las trabajar en las gráficas de los colaboradores en el cluster
class CargarGraficaProjectoClusterObj {
	dataCB = {};
	idContenedorCB = "";
	typesOcultar = [];
	showRelation = true;

	actualizarGraficaColaboradores = () => {
		AjustarGraficaArania(this.dataCB, this.idContenedorCB, this.typesOcultar, this.showRelation);
	};

	CargarGraficaColaboradores = (pIdGrupo, parametros, idContenedor, mostrarCargando = false) => {
		var url = servicioExternoBaseUrl + "Hercules/DatosGraficaColaboradoresGrupo";
		var self = this;
		var arg = {};
		arg.pIdGrupo = pIdGrupo;
		arg.pParametros = parametros;
		arg.pMax = $('#numColaboradoresCluster').val();
		$('#' + idContenedor).empty();
		if (mostrarCargando) {
			MostrarUpdateProgress();
		}

		let optionsRelations = ["relation_project", "relation_document"];

		$.get(url, arg, function (data) {
			// Establecer los valores en la variable externa
			self.dataCB = data;
			self.idContenedorCB = idContenedor;

			self.actualizarGraficaColaboradores();
			if (mostrarCargando) {
				OcultarUpdateProgress();
			}
		});

	};

};

// Creamos un nuevo objeto
var newGrafProjClust = new CargarGraficaProjectoClusterObj();


// Función a la que se llama para seleccionar qué elementos de las relaciones mostrar
function actualizarTypesClusterOcultar(type) {
	if (type == "relation_todas") {
		newGrafProjClust.typesOcultar = [];
	} else {
		newGrafProjClust.typesOcultar = [type];
	}
	newGrafProjClust.actualizarGraficaColaboradores();
}


// función para actualizar la gráfica de colaboradores
function ActualizarGraficaClusterolaboradoresCluster(typesOcultar = [], showRelation = true) {
	AjustarGraficaArania(dataCB, idContenedorCB, typesOcultar, showRelation);
}

// Comportamiento página proyecto
var comportamientoPopupCluster = {
	tabActive: null,
	init: function (clusterObj) {
		this.config();
		let paramsCl = this.workCO(clusterObj)
		let profiles = this.setProfiles(clusterObj)

		// Iniciar el listado de usuarios
		buscadorPersonalizado.init($('#INVESTIGADORES').val(), "#clusterListUsers", "searchCluster=" + paramsCl, null, "profiles=" + JSON.stringify(profiles) + "|viewmode=cluster|rdf:type=person", $('inpt_baseUrlBusqueda').val(), $('#inpt_proyID').val());

		//TODO
		//var parametros = ObtenerHash2()+"&" + buscadorPersonalizado.filtro;
		//newGrafProjClust.CargarGraficaProjectoClusterObj("", parametros, 'colaboratorsgraphCluster');

		return;
	},
	config: function () {
		var that = this;

		//Métodos colaboradores
		$('#removeNodesCollaboratorsCluster').unbind().click(function (e) {
			e.preventDefault();
			var numColaboradores = parseInt($('#numColaboradoresCluster').val());
			if (numColaboradores > 10) {
				$('#numColaboradoresCluster').val((numColaboradores - 10));
			}
			$('#numNodosCollaboratorsCluster').html($('#numColaboradoresCluster').val());
			var parametros = ObtenerHash2() + "&" + buscadorPersonalizado.filtro;
			newGrafProjClust.CargarGraficaProjectoClusterObj("", parametros, 'colaboratorsgraphCluster', true);
		});

		$('#addNodesCollaboratorsCluster').unbind().click(function (e) {
			e.preventDefault();
			var numColaboradores = parseInt($('#numColaboradoresCluster').val());
			$('#numColaboradoresCluster').val((numColaboradores + 10));
			$('#numNodosCollaboratorsCluster').html($('#numColaboradoresCluster').val());
			var parametros = ObtenerHash2() + "&" + buscadorPersonalizado.filtro;
			 CargarGraficaProjectoClusterObj("", parametros,'colaboratorsgraph',true);
			 newGrafProjClust.CargarGraficaProjectoClusterObj("", parametros, 'colaboratorsgraphCluster', true);
		});
		//Fin métodos colaboradores

		return;
	},

	/*
	* Convierte el objeto del cluster a los parámetros de consulta 
	*/
	workCO: function (clusterObj) {

		let results = null
		if (clusterObj && clusterObj.profiles) {
			results = clusterObj.profiles.map(e => {
				let terms = (e.terms.length) ? e.terms.map(itm => '<' + itm + '>').join(',') : "<>"
				let tags = (e.tags.length) ? e.tags.map(itm => "'" + itm + "'").join(',') : "''"
				return terms + '@@@' + tags
			}).join('@@@@')
		}

		return results
	},

	/*
	* Convierte los profiles en json 
	*/
	setProfiles: function (clusterObj) {

		let results = null
		if (clusterObj && clusterObj.profiles) {
			clusterObj.profiles.forEach((e, i) => {

			})
			results = clusterObj.profiles.map((e, i) => (
				{
					[e.name.replace(/[^a-z0-9_]+/gi, '-').replace(/^-|-$/g, '').toLowerCase() + "-" + i]: e.name
				}
			))
		}

		return results
	}


};

/**
* Clase que contiene la funcionalidad del modal de los TAGS para el Cluster
*/
class ModalSearchTags {
	constructor() {
		this.body = $('body')
		this.modal = this.body.find('#modal-anadir-topicos-cluster')
		this.inputSearch = this.modal.find('#tagsSearchModalCluster')
		this.results = this.modal.find('.ac_results')
		this.resultsUl = this.results.find('ul')
		this.timeWaitingForUserToType = 750; // Esperar 0.75 segundos a si el usuario ha dejado de escribir para iniciar búsqueda
		this.tagsWrappper = this.modal.find('.tags ul')
		this.ignoreKeysToBuscador = [37, 38, 39, 40, 46, 8, 32, 91, 17, 18, 20, 36, 18, 27];

		// Inicializa la funcionalidad del buscador de TAGS
		this.inputSearchEnter()

		/* if (window.location.hostname == 'depuracion.net' || window.location.hostname.includes("localhost")) {
			var urlSTAGS = new URL(servicioExternoBaseUrl + 'servicioexterno/' + uriSearchTags)
		} */
	}

	/**
	 * Inicia la funcionalidad del modal
	 */
	init() {
		this.modal.modal('show') 
	}

	/**
	 * Funcionalidad para cuando se introduce un valor en el buscador
	 */
	inputSearchEnter() {
		let _self = this
		this.inputSearch.off('keyup').on('keyup', function(e) {
			let inputVal = this.value

			if (inputVal.length <= 1) 
			{
				_self.hiddenResults()
			}
			else {
				// Valida si el valor introducirdo en el input es 'válido'
				if (_self.validarKeyPulsada(e) == true) {
					// Limpia el 'time' para reinicializar la espera para iniciar la búsqueda de TAGS
					clearTimeout(_self.timer)
					// Espera 0.5s para realizar la búsqueda de TAGS
					_self.timer = setTimeout(function () {
						_self.hiddenResults()

						_self.searchCall(inputVal).then((data) => {
							// Muestra el selector 
							_self.results.show()
							if (data.length > 0) {
								// Pinta los resultados
								let resultHTML = data.map(e => {
									return '<li class="ac_even">' + e + '</li>'
								})
								_self.resultsUl.html(resultHTML.join(''))
								_self.addTagClick()

							} else {
								_self.mostrarPanelSinResultados()
							}
							
						})

					}, _self.timeWaitingForUserToType);
				}               
			}
		})
	}

	/**
	 * Oculta y borra los resultados del API
	 */
	hiddenResults() {
		this.resultsUl.html('')
		this.results.hide()
	}

	/**
	 * Muestra un texto "sin resultados" cuando no hay resultados del API
	 */
	mostrarPanelSinResultados() {
		this.resultsUl.html('<li>Sin resultados</li>')
	}

	/**
	 * Método que genera un inicia el evento de añadir el tag (al hacer 'click') cuando se 
	 * ha seleccionado un elemento de la lista de opciones disponibles 
	 */
	addTagClick() {
		let _self = this
		// Evento para el click
		this.resultsUl.off('click').on('click', 'li', function(e) {
			let texto = $(this).text()

			if (texto != "Sin resultados") {
				_self.addTag(texto) // Añade el texto
				_self.hiddenResults() // Vacía los resultados de la búsqueda
				_self.inputSearch.val('') // Vacía el campo de búsqueda
			}

		})
		// Delete list item  
	}

	/**
	 * Añade un tag al modal
	 * param texto, texto del tag a añadir
	 */
	addTag(texto) {
		let item = $(`<li>
						<a href="javascript: void(0);">
							<span class="texto">` + texto + `</span>
						</a>
						<span class="material-icons cerrar">close</span>
					</li>`)
		this.tagsWrappper.append(item)
		
		item.off('click').on('click', '.cerrar', function(e) {
			let li = $(this).parent().remove()
		})
	}

	/**
	 * Remove all tags html
	 */
	removeTags() {
		this.tagsWrappper.html('')
	}

	/**
	 * Método que genera un evento para el botón "guardar" y devuelve el número de TAGS añadidas
	 * return promise (array) con la lista de resultados 
	 */
	closeBtnClick() {
		let _self = this
		return new Promise((resolve, reject) => {

			this.modal.find('.btnclosemodal').off('click').on('click', function(e) {
			
				let result = new Array()
				_self.tagsWrappper.find('li .texto').each((i, item) => {
					result.push(item.innerText)
				})
				resolve(result)
			})
		})
	}


	/**
	 * Comprobará la tecla pulsada, y si no se encuentra entre las excluidas, dará lo introducido por válido devolviendo true
	 * Si se pulsa una tecla de las excluidas, devolverá false y por lo tanto el metabuscador no debería iniciarse
	 * param {any} event: Evento o tecla pulsada en el teclado
	 * return bool, devuelve si la tecla pulsada es válida o no
	 */
	validarKeyPulsada (event) {
		const keyPressed = this.ignoreKeysToBuscador.find(key => key == event.keyCode);
		if (keyPressed) {
			return false
		}
		return true;
	}

	/**
	 * Realiza la petición ajax (GET) para buscar los tags sugeridos en el input
	 * param {string} inputVal: Texto para la búsqueda
	 * return promise (array) con la lista de resultados 
	 */
	searchCall (inputVal) {
		// Set the url parameters
		urlSTAGS.searchParams.set('tagInput', inputVal)

		return new Promise((resolve) => {
			$.get(urlSTAGS.toString(), function (data) {
				resolve(data)
			});
		})
	}
}

function CompletadaCargaRecursosCluster()
{	
	if(stepsCls!=null && stepsCls.data!=null)
	{
		stepsCls.data.pPersons=$('#clusterListUsers article.investigador').toArray().map(e => {return $(e).attr('id')});
		$.post(urlCargarPerfiles, stepsCls.data, function(data) {
			console.log('ok')
		});
	}
}