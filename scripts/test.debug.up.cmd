@echo off
docker-compose --file docker-compose.test.yml --file docker-compose.test.debug.yml --project-name test up --detach
docker-compose --file docker-compose.test.yml --file docker-compose.test.debug.yml --project-name test logs --follow
