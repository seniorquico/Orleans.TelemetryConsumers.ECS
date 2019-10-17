# Debugging Tests in Docker

Open two Command Prompts. In the first Command Prompt, execute the following commands:

```cmd
cd %PROJECT_DIR%
.\scripts\test.debug.up.cmd
```

In the second Command Prompt, execute the following commands:

```cmd
cd %PROJECT_DIR%
.\scripts\test.debug.attach.cmd
```

This will start an interactive Bash prompt in the project container. In the Bash prompt, execute the following commands:

```bash
apt update && apt install -y --no-install-recommends unzip
curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/vsdbg
```

# Localstack + AWS ECS Local Endpoint Simulator

docker container run -it --rm -v "$( pwd ):/usr/src/app" -w /usr/src/app golang:stretch /bin/bash
  git clean -f -d -x
  git reset --hard
  go get -u golang.org/x/lint/golint
  make bin/linux-amd64/local-container-endpoints

docker build -t amazon/amazon-ecs-local-container-endpoints:latest .

docker-compose -f docker-compose.test.yml up --abort-on-container-exit
(docker-compose -f docker-compose.test.yml down)

docker exec -it orleanstelemetryconsumerselasticcontainerservice_busybox_1 /bin/bash
  apt update && apt dist-upgrade -y && apt install -y --no-install-recommends curl jq python3-pip python3-setuptools && pip3 install wheel --upgrade --user && pip3 install awscli --upgrade --user && export PATH=$PATH:/root/.local/bin
  aws --version
  aws s3 ls --endpoint http://localstack:4572
  curl -sS http://169.254.170.2/creds | jq .
  curl -sS http://169.254.170.2/v3 | jq .
  curl -sS http://169.254.170.2/v3/stats | jq .
  curl -sS http://169.254.170.2/v3/task | jq .
  curl -sS http://169.254.170.2/v3/task/stats | jq .
