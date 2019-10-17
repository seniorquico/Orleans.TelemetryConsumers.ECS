@echo off
docker-compose --file docker-compose.build.yml --project-name test build --force-rm
docker-compose --file docker-compose.test.yml --project-name test up --abort-on-container-exit
