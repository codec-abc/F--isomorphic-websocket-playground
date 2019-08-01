# Dependencies
* .NET Core
* Yarn
* Node.js

# Build
To build the whole application, first build the client. It will output a single js file (`bundle.js`) in the `wwwroot` directory.

# Client Build
The first time you need to install dependencies

```
yarn install
```

To compile once you can use `webpack-cli`. To compile on change use `webpack --watch`.
To install both tools you can do the following:

```
yarn global add webpack-cli
yarn global add webpack
```

Don't forget to add yarn global directory to path so these executable can be found.

# Server Build

Once the client is built, you can build the server. Just type :
```
dotnet run
```
To run the server or
```
dotnet watch run
```
to compile on change

# Google App Engine deployment

[Follow guide here](https://cloud.google.com/appengine/docs/flexible/dotnet/quickstart) and [here.](https://cloud.google.com/appengine/docs/flexible/dotnet/testing-and-deploying-your-app)

# To build and run on Docker

Just type:
```
docker build -t aspnetapp .
docker run -d -p 8080:5000 --name myapp aspnetapp
```