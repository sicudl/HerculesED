var urlExportacionCV = "https://localhost:5002/ExportadoCV/";
//var urlExportacionCV = "http://serviciosedma.gnoss.com/editorcv/ExportadoCV/";

var exportacionCV = {
    idUsuario: null,
    init: function() {
        this.config();
        //TODO this.idUsuario = $('.contenido-cv').attr('userID');
		
        return;
    },
	config: function(){
		$('body').addClass('page-cv');
		//TODO
		//TODO cambiar
        $('body').append(`
		<style>
		.entityauxcontainer>.entityaux{display:none!important;}
		.entitycontainer>.entity{display:none!important;}
		.form-group.multiple input,.form-group.entitycontainer:not(.multiple) input{display:inline;height:38px;}
		.form-group.multiple .acciones-listado-edicion{width:30%;float:right;margin-bottom:0px;}
		.form-group.multiple .acciones-listado-edicion{width:auto;float:none;}
		.form-group.multiple:not(.entitycontainer)>div {display:flex;}
		.form-group.entitycontainer:not(.multiple)>div {display:flex;}
		.form-group.multiple.entityauxcontainer>div {
			display: block;
		}
		.form-group-date.material-icons {
				position: absolute;
				top: 42px;
				left: 10px;
				color: var(--c-secundario);
			}
		.custom-form-row .form-group {
			position: relative;
		}
		input.form-group-date {
			padding-left: 45px;
		}
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.mini, 
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.mini {
			display: block;
		}
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group.resaltado p {
			font-weight: 500;
		}
		.page-cv .resource-list .resource.activo .middle-wrap .content-wrap .description-wrap .group.resaltado p {
			font-weight: 300;
		}
		
		.page-cv #modal-eliminar {
			z-index: 1501 !important;
		}
		.page-cv .modal-backdrop.show.modal-eliminar {
			z-index: 1500 !important;
		}
		
		.page-cv #modal-editar-entidad {
			z-index: 1062 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad {
			z-index: 1061 !important;
		}
		
		.page-cv #modal-editar-entidad-0 {
			z-index: 1064 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-0 {
			z-index: 1063 !important;
		}
		
		.page-cv #modal-editar-entidad-0 .modal-dialog {
			width: 100%;
			max-width: 1180px;
		}
		
		.page-cv #modal-editar-entidad-1 {
			z-index: 1066 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-1 {
			z-index: 1065 !important;
		}
		.page-cv #modal-editar-entidad-1 .modal-dialog {
			width: 100%;
			max-width: 1080px;
		}
		
		.page-cv #modal-editar-entidad-2 {
			z-index: 1068 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-2 {
			z-index: 1067 !important;
		}
		.page-cv #modal-editar-entidad-2 .modal-dialog {
			width: 100%;
			max-width: 980px;
		}
		
		.page-cv #modal-editar-entidad-3 {
			z-index: 1070 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-3 {
			z-index: 1069 !important;
		}
		.page-cv #modal-editar-entidad-3 .modal-dialog {
			width: 100%;
			max-width: 880px;
		}
		
		.page-cv #modal-editar-entidad-4 {
			z-index: 1072 !important;
		}
		.page-cv .modal-backdrop.show.modal-editar-entidad-4 {
			z-index: 1071 !important;
		}
		.page-cv #modal-editar-entidad-4 .modal-dialog {
			width: 100%;
			max-width: 780px;
		}
		
		.page-cv #modal-anadir-autor {
			z-index: 1500 !important;
		}
		
		.page-cv .modal-backdrop.show.modal-anadir-autor {
			z-index: 1499 !important;
		}
		
		
		.select2-container--default .select2-results__option[aria-disabled=true] {
			display: none;
		}

		.panel-group.pmd-accordion article.activo .material-icons.arrow {
			-ms-transform: rotate(180deg);
			transform: rotate(180deg);
		}

		.panel-group.pmd-accordion a[aria-expanded="true"] .pmd-accordion-arrow {
			-ms-transform: rotate(0deg)!important;
			transform: rotate(0deg)!important;
		}
		
		.panel-group.pmd-accordion a[aria-expanded="false"] .pmd-accordion-arrow {
			-ms-transform: rotate(180deg)!important;
			transform: rotate(180deg)!important;
		}
		
		.entityaux ul li .faceta:not(.last-level)::before {
			background: #ccc;
		}

		.topic .item.added {
			display: none !important;
		}

		.topic .item.aux input {
			position: absolute;
			top: 0;
			width: 50%;
			right: 94px;
		}

		.topic a.btn.btn-outline-grey.add {
			height: 38px;
		}
		
		.entityaux ul li .faceta.last-level.selected.lock,
		.entityaux ul li .faceta.last-level.selected.lock::before {
			background: #ccc;
			cursor:not-allowed;
		}
		:root{
			--c-gris-claro: #BBB;
			--c-gris-oscuro: #555;
		}
		
		.list-wrap ul li.background-oscuro { background-color: var(--c-gris-claro) !important; border: 1px solid var(--c-gris-oscuro) !important; }
		.list-wrap ul li.background-oscuro a { color: var(--c-gris-oscuro); }
		.list-wrap ul li.background-oscuro .material-icons { color: var(--c-gris-oscuro); }


		.ac_results{
			top: 80px!important;
			left: 0px!important;
		}
		
		
		.topic .item.aux .ac_results{
			top: 40px!important;
			left: calc(50% - 94px)!important;
		}
		
		.formulario-edicion .nav.nav-tabs {
			position: absolute;
			top: 0;
			left: 0;
			right: 0;
			padding: 0px 40px;
			padding-top: 20px;
		}
		
		#modal-editar-entidad .modal-content .modal-body {
			padding: 60px 40px;
		}

		.resource-list.translationEnView .translation-en {
			display: block;
		}
		
		.resource-list.translationCaView .translation-ca {
			display: block;
		}
		
		.resource-list.translationEuView .translation-eu {
			display: block;
		}
		
		.resource-list.translationGlView .translation-gl {
			display: block;
		}
		
		.resource-list.translationFrView .translation-fr {
			display: block;
		}
		
		.page-cv .resource-list .resource .middle-wrap .content-wrap .description-wrap .group ul li{
			margin-bottom: 0;
		}
		
		#collapse-autores .collapse-toggle, #collapse-lista-autores .user-list .collapse-toggle {
			font-size: inherit;
			font-weight: 500;
			text-transform: initial;
			color: var(--c-primario);
		}
		
		#collapse-lista-autores .user-list > ul > li {
			padding-right: 12px;
		}
		
		#collapse-lista-autores .user-list {
			display: flex;
			flex-wrap: wrap;
			align-items: center;
		}

        /* WYSIWYG Editor */
        .edmaTextEditor {
            width: 100%;
            min-height: 18rem;
           
            border-top: 2px solid #4a4a4a;
            border-radius: 3px;
            margin: 2rem 0;
        }

        .edmaTextEditor .toolbar .line {
            display: flex;
            border: 1px solid #e2e2e2;
        }

        .edmaTextEditor .toolbar .line .box {
            display: flex;
        }

        .edmaTextEditor .toolbar .line .box .editor-btn {
            transition: 0.2s ease all;
        }

        .edmaTextEditor .toolbar .line .box .editor-btn:hover {
            background-color: #f0f0f0;
            cursor: pointer;
        }

        .edmaTextEditor .toolbar .line .box .editor-btn.active {
            background-color: #e1e1e1;
        }

        .edmaTextEditor .toolbar .line .box .editor-btn.icon img {
            width: 16px;
            padding: 9px;
            box-sizing: content-box;
        }

        .visuell-view[placeholder]:empty:before {
            content: attr(placeholder);
            color: #AAA; 
        }
        
        .visuell-view[placeholder]:empty:focus:before {
            content: "";
        }

        .edmaTextEditor .visuell-view {
            line-height: 1.5;
            box-sizing: border-box;
            outline: none;
            display: inline-block;
            min-height: inherit;
            overflow-y: auto;
            width: inherit;
            overflow-x: auto;
        }
        .edmaTextEditor[disabled] .toolbar {
            pointer-events:none;

        }
        .edmaTextEditor[disabled] {
            opacity: 0.5;
            background: #CCC;
        }

		</style>`);
	},
	//Métodos de pestañas
    cargarCV: function() {
        var that = this;
		MostrarUpdateProgressTime(0);
        //MostrarUpdateProgress();
		//TODO cambiar url
        //$.get(urlExportacionCV + 'GetAllTabs?userID=' + that.idUsuario + "&pLang=" + lang, null, function(data) {
        $.get(urlExportacionCV + 'GetAllTabs?userID=d7711fd2-41d2-464b-8838-e42c52213927&pLang=es', null, function(data) {
            //recorrer items y por cada uno	
			for(var i=0;i<data.length;i++){
				var id = 'x' + RandomGuid();
				var contenedorTab=`<div class="panel-group pmd-accordion" id="datos-accordion" role="tablist" aria-multiselectable="true">
										<div class="panel">
											<div class="panel-heading" role="tab" id="datos-tab">
												<p class="panel-title">
													<a data-toggle="collapse" data-parent="#datos-accordion" href="#datos-panel" aria-expanded="true" aria-controls="datos-tab" data-expandable="false" class="">
														<span class="texto">${data[i].title}</span>
														<span class="material-icons pmd-accordion-arrow">keyboard_arrow_down</span>
													</a>
												</p>
											</div>
											<div about="${id}">
												<div class="row cvTab">
													<div class="col-12 col-contenido">
													</div>
												</div>
											</div>
										</div>
									</div>`
				$('.cabecera-cv').append( $(contenedorTab));		
				edicionCV.printTab(id, data[i]);
			}			
            OcultarUpdateProgress();
			
			$('.resource-list.listView .resource .wrap').css("margin-left", "70px")
			checkAllCVWrapper();
        });
        return;
    }
};

$(window).on('load', function(){
	exportacionCV.cargarCV();
	
	var myForm = `<form action="" id="myForm" method="post">
					<button type="submit" >Exportar</button>
				</form>`;
	$('#containerCV').append(myForm);
	
	$(function() {
		$('#myForm').submit(function(e) {
			e.preventDefault();
			e.stopPropagation();
			
			var listaId = "";
			$('.resource-list .custom-control-input:checkbox:checked').each(function(){
				listaId += (this.checked ? $(this).val()+"@@@" : "")
			});
			
			listaId = listaId.slice(0,-3);			
			
			$.ajax({
				type: 'POST',
				url: urlExportacionCV+'GetCV',
				dataType: 'json',
				data: {
					userID: 'd7711fd2-41d2-464b-8838-e42c52213927', 
					lang: lang,
					listaId: listaId
				}
			});
			return false;
		});
	});
});

function checkAllCVWrapper(){
	$('.checkAllCVWrapper input[type="checkbox"]').off('click').on('click', function(e) {
		$(this).closest('.panel-body').find('article div.custom-checkbox input[type="checkbox"]').prop('checked',$(this).prop('checked'));
	});
	
	$('.checkAllCVWrapper input[type="checkbox"]').closest('.panel-body').find('article div.custom-checkbox input[type="checkbox"]').off('change').on('change', function(e) {
		if(!$(this).prop('checked')){
			$(this).closest('.panel-body').find('.checkAllCVWrapper input[type="checkbox"]').prop('checked', false);
		}
	});
}
	
edicionCV.printPersonalData=function(data) {        
	return 'SECCION DE DATOS PERSONALES';
};

edicionCV.printTabSection= function(data) {
	//Pintado sección listado
	//css mas generico
	var id = 'x' + RandomGuid();
	var id2 = 'x' + RandomGuid();

	var expanded = "";
	var show = "";
	if (data.items != null) {
		if (Object.keys(data.items).length > 0) {
			//Desplegado
			expanded = "true";
			show = "show";
		} else {
			//No desplegado	
			expanded = "false";
		}
		//TODO texto ver items
		var htmlSection = `
		<div class="panel-group pmd-accordion" section="${data.identifier}" id="${id}" role="tablist" aria-multiselectable="true">
			<div class="panel">
				<div class="panel-heading" role="tab" id="publicaciones-tab">
					<p class="panel-title">
						<a data-toggle="collapse" data-parent="#${id}" href="#${id2}" aria-expanded="${expanded}" aria-controls="${id2}" data-expandable="false">
							<span class="material-icons pmd-accordion-icon-left">folder_open</span>
							<span class="texto">${data.title}</span>
							<span class="numResultados">(${Object.keys(data.items).length})</span>
							<span class="material-icons pmd-accordion-arrow">keyboard_arrow_up</span>
						</a>
					</p>
				</div>
				<div id="${id2}" class="panel-collapse collapse" role="tabpanel">				
					<div id="situacion-panel" class="panel-collapse collapse show" role="tab-panel" aria-labelledby="situacion-tab" style="">
						<div class="panel-body">
							<div class="acciones-listado acciones-listado-cv">
								<div class="wrap">
									<div class="checkAllCVWrapper" id="checkAllCVWrapper">
										<div class="custom-control custom-checkbox">
											<input type="checkbox" class="custom-control-input" id="checkAllResources_${id2}">
											<label class="custom-control-label" for="checkAllResources_${id2}">
											</label>
										</div>
									</div>
								</div>
								<div class="wrap">
									<div class="ordenar dropdown">${this.printOrderTabSection(data.orders)}</div>
									<div class="buscador">
										<div class="fieldsetGroup searchGroup">
											<div class="textoBusquedaPrincipalInput">
												<input type="text" class="not-outline txtBusqueda" placeholder="${GetText('CV_ESCRIBE_ALGO')}" autocomplete="off">
												<span class="botonSearch">
													<span class="material-icons">search</span>
												</span>
											</div>
										</div>
									</div>
								</div>
							</div>
							<div class="resource-list listView">
								<div class="resource-list-wrap">
									${this.printHtmlListItems(data.items)}
									<div class="panNavegador">
										<div class="items dropdown">
											<a class="dropdown-toggle" data-toggle="dropdown" aria-expanded="true">
												<span class="texto" items="5">Ver 5 items</span>
											</a>
											<div class="dropdown-menu basic-dropdown dropdown-menu-right" x-placement="bottom-end">
												<a href="javascript: void(0)" class="item-dropdown" items="5">Ver 5 items</a>
												<a href="javascript: void(0)" class="item-dropdown" items="10">Ver 10 items</a>
												<a href="javascript: void(0)" class="item-dropdown" items="20">Ver 20 items</a>
												<a href="javascript: void(0)" class="item-dropdown" items="50">Ver 50 items</a>
												<a href="javascript: void(0)" class="item-dropdown" items="100">Ver 100 items</a>
											</div>
										</div>
										<nav>
											<ul class="pagination arrows">
											</ul>
											<ul class="pagination numbers">	
												<li class="actual"><a href="javascript: void(0)" page="1">1</a></li>
											</ul>
										</nav>
									</div>
								</div>
							</div>
						</div>
					</div>					
				</div>
			</div>
		</div>`;
		return htmlSection;
	}
};

edicionCV.printHtmlListItem= function(id, data) {
	let openAccess="";
	if (data.isopenaccess) {
		openAccess = "open-access";
	}
	var htmlListItem = `<article class="resource success ${openAccess}" >
							<div class="custom-control custom-checkbox">
								<input type="checkbox" class="custom-control-input" id="check_resource_${data.identifier}"  value="${id}">
								<label class="custom-control-label" for="check_resource_${data.identifier}"></label>
							</div>
							<div class="wrap">
								<div class="middle-wrap">
									${this.printHtmlListItemOrders(data)}
									<div class="title-wrap">
									</div>
									<div class="title-wrap">
										<h2 class="resource-title">
											<a href="#" data-id="${id}" internal-id="${data.identifier}">${data.title}</a>
										</h2>
										${this.printHtmlListItemEditable(data)}	
										${this.printHtmlListItemIdiomas(data)}
										<span class="material-icons arrow">keyboard_arrow_down</span>
									</div>
									<div class="content-wrap">
										<div class="description-wrap">
											${this.printHtmlListItemPropiedades(data)}
										</div>
									</div>
								</div>
							</div>
						</article>`;
	return htmlListItem;
};