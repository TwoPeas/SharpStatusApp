# SharpStatusApp

This is a playground, of sorts, for me to try out some options around "modernish" web dev. I'm hoping to patternize and extract reusable (even if copy-paste) blocks out of this for micro ISV experiments.

## Running in Docker locally:

You can create a `docker` image of the application by running `docker build -t sharpstatusapp`. This will give you a new image tagged `sharpstatusapp`. 

The application relies on PostgreSQL for storage. You can bring up a local instance of PostgreSQL and PgAdmin by running `docker-compose -up` which will start these services for you. Note, this will automatically 
create a storage volume (you'll see them as subdirectories) in this directory. This persists your Postgres data when the container is terminated.

You'll then want to run your app. By default, the app is going to try and read a secret from Google Secret Manager. Which won't work for you. So, you'll need to override the envrionment 
variable with your own connection string. You can do that as part of the `docker run` command, being sure to also tell it to use the host network. Note, this binds the docker container to
the host network and you really should only do that when you're running this locally.

```cmd
docker run --rm -it -p 8585:80 -e "ConnectionStrings__DefaultConnection=Server=localhost;Port=5432;Database=sharpstatus;User Id=root;Password=root" --network "host" sharpstatusapp
```

This will start the docker container and then stop it automatically when you terminate (ctrl+c) the app.

Of course, this is all kind of messy. So, if you want to bring up the entire stack at once then you can run the Docker Compose with the configuration that will also start the app for you

```
docker-compose -f docker-compose.test.yaml up
```

