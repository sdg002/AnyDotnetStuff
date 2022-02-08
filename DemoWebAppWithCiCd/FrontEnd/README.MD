
# NPM initialization
```
npm init
```

# NPM Express installation
```
npm install express -save
```

# Create an index.js
```
var express = require("express");
 
var app = express();
 
app.use(express.static('public'));
 
 
var server = app.listen(8081, function(){
    var port = server.address().port;
    console.log("Server started at http://localhost:%s", port);
});
```

# Structure of package.json

```
{
  "name": "simplestatic",
  "version": "1.0.0",
  "description": "this is a simple static web site",
  "main": "index.js",
  "scripts": {
    "start": "node index.js",
    "test": "echo \"Error: no test specified\" && exit 1"
  },
  "author": "",
  "license": "ISC",
  "dependencies": {
    "express": "^4.17.2"
  }
}

```
# Create a very skeletal default.html
```
<html>
    <body>
        <h1>This is a demo page</h1>
        <hr/>    
    </body>

</html>
```

# How to start the web server?
```
node index.js
```
or
```
npm start
```