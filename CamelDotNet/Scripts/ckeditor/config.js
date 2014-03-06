/**
 * @license Copyright (c) 2003-2013, CKSource - Frederico Knabben. All rights reserved.
 * For licensing, see LICENSE.html or http://ckeditor.com/license
 */

CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here.
	// For the complete reference:
	// http://docs.ckeditor.com/#!/api/CKEDITOR.config
    CKEDITOR.config.language = "en"

	// The toolbar groups arrangement, optimized for two toolbar rows.
	config.toolbarGroups = [
		{ name: 'clipboard',   groups: [ 'clipboard', 'undo' ] },
		{ name: 'editing',     groups: [ 'find', 'selection', 'spellchecker' ] },
		{ name: 'links' },
		{ name: 'insert' },
		{ name: 'forms' },
		{ name: 'tools' },
		{ name: 'document',	   groups: [ 'mode', 'document', 'doctools' ] },
		{ name: 'others' },
		'/',
		{ name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
		{ name: 'paragraph',   groups: [ 'list', 'indent', 'blocks', 'align' ] },
		{ name: 'styles' },
		{ name: 'colors' },
		{ name: 'about' }
	];

	// Remove some buttons, provided by the standard plugins, which we don't
	// need to have in the Standard(s) toolbar.
	config.removeButtons = 'Underline,Subscript,Superscript';
	
	config.extraPlugins = 'wordcount,codemirror,autogrow,print';
	
	config.autoGrow_maxHeight = 500;
	
	config.removePlugins = 'resize';
	
	// If you want to use a theme set the theme here 
	// config.codemirror_theme : 'rubyblue';

	// Extra Option to disable/enable Automatic Code Formatting every time the source view is opened (By Default its enabled)
	// config.codemirror_autoFormatOnStart : true; 
};

// When opening a dialog, its "definition" is created for it, for
// each editor instance. The "dialogDefinition" event is then
// fired. We should use this event to make customizations to the
// definition of existing dialogs.
CKEDITOR.on( 'dialogDefinition', function( ev )
	{
		// Take the dialog name and its definition from the event
		// data.
		var dialogName = ev.data.name;
		var dialogDefinition = ev.data.definition;
		
		var tinyMCELinkList = new Array(
			// Name, URL
			["Domain", "http://site.ru/"],
			["About", "http://site.ru/about"],
			["Dealers", "http://site.ru/dealers"],
			["Contacts", "http://site.ru/contacts"],
			["Service", "http://site.ru/Service/zamer-okon-dverei"],
			["\u21d2Edit this in config.js", "http://site.ru/xxx"],				
			["Article-xxxxx-xxxx-xxx", "http://site.ru/Article/xxxxx-xxxx-xxx"]
		);
 
		// Check if the definition is from the dialog we're
		// interested on (the "Link" dialog).
		if ( dialogName == 'link' )
		{
			// Get a reference to the "Link Info" tab.
			// CKEDITOR.dialog.definition.content
			var infoTab = dialogDefinition.getContents( 'info' );
 
			// Add a new UI element to the info box.
			// Use a JSON array named items to grab the list.
			// Modify the onChange event to update the real URL field.
			infoTab.add(
			{
				type : 'vbox',
				id : 'localPageOptions',
				children : [
				{
					type : 'select',
					label : 'Select the page from within the site to link to.',
					id : 'localPage',
					title : 'Select the page from within the site to link to.',
					items: tinyMCELinkList,
					onChange : function(ev) {
						var diag = CKEDITOR.dialog.getCurrent();
						var url = diag.getContentElement('info','url');
						url.setValue(ev.data.value);
					}
				}]
			}
			);
			
			// Rewrite the 'onFocus' handler to always focus 'url' field.
			dialogDefinition.onFocus = function()
			{
				var urlField = this.getContentElement( 'info', 'url' );
				urlField.select();
			};
		}
	});