function augurData ( json ) {
	$('#did').val(json.device.ID);
}

$(function () {
	var epsSocket = new WebSocket("ws://starstuffindustries.com:8181/");
	//var epsSocket = new WebSocket("ws://localhost:8080/");

	epsSocket.onopen = function (event) {
		$('#results').html('Connected to server...');
	};
	epsSocket.onerror = function (error) {
		console.log('WebSocket Error ' + error);
	};
	epsSocket.onmessage = function(e) {
		var obj = JSON.parse(e.data);
		console.log(obj);
		if (e.data.length) {
			$('#results').html('Connected to server...');
			$('#dname').html(obj.Name);
			$('#dstanding').html(obj.Standing);
		}
	};
	epsSocket.onclose = function() {
		console.log('Disconnected.');
		$('#results').html('Disconnected. The server didn\'t respond.');
		if ( $.browser.mozilla ) {
			$('#results').html( "Attention: Some Firefox versions have problems with websockets. To use Firefox with Eve Public Standing navigate your browser to about:config and search for 'network.websocket.extensions.permessage-deflate'. Double-click this setting to change it to false." );
		}
	};

	$("#sub-apiform").on("click", function () {
		var keyid = $('#keyid').val();
		var vcode = $('#vcode').val();
		var cname = $('#apiname').val();
		var dID = $('#did').val();
		if (vcode.length == 64 && (keyid.length == 5 || keyid.length == 6 || keyid.length == 7) && (cname.length >= 3 || cname.length <= 24)) {
			var json = {"keyid":keyid.toString(), "vcode":vcode.toString(), "cname":cname.toString(), "did":dID};
			epsSocket.send(JSON.stringify(json));
			$('#results').html('Submitted contacts for '+cname+'.');
		} else {
			$('#results').html('Error: API details failed validation, please try again.');
		}
	});
	$("#sub-search").on("click", function () {
		var cname = $('#cname').val();
		if (cname.length > 0) {
			var json = {"term":cname.toString()};
			epsSocket.send(JSON.stringify(json));
			$('#results').html('Searching for '+cname+'.');
		} else {
			$('#results').html('Error: You must enter a name to search for.');
		}

	});
});
