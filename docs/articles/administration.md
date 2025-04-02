# Administration

The admin portal is where you will manage the projects that Volt Projects will build and serve documentation for.

It can be accessed through the admin URL:

```
https://<Volt Projects URL>/admin/
```

![Admin Portal](~/assets/images/admin-main.webp)

## Projects

The base of everything Volt Projects, a Project. Projects contain basic details on a project, the mains ones being the name and the Git URL of the repo.

![Project](~/assets/images/admin-project.webp)

### Project Versions

A project version defines a version tag that Volt Projects will serve, it also were the build information such as path in the git repo were the docs exist and the doc builder to use is defined.

![Project Version](~/assets/images/admin-project-version.webp)

A project can have multiple project versions, but needs at least one default (usually the "latest" version of your docs).

Pre-Build commands can also be set here. These are commands that will be executed before the main doc builder is ran. Commands will be executed in order.

## Build Schedules

Once you have configured a project, you can create a build schedule for it. Build schedule tell builder how often to re-build a project (if required). The schedule is set using a cron format.

![Build Schedule](~/assets/images/admin-build-schedules.webp)

Flagging "Ignore Build Events" as true will cause builder to always do a complete re-build and upload of project files every single time the job fires. This can be useful in dev, but shouldn't really be used in production.
