#!/bin/bash

( sleep 60 ; \
rabbitmqctl add_user admin admin 2>/dev/null ; \
rabbitmqctl set_user_tags admin administrator ; \
rabbitmqctl set_permissions -p / admin ".*" ".*" ".*" ; \

rabbitmqctl add_user controller controller 2>/dev/null ; \
rabbitmqctl add_user noolite noolite 2>/dev/null ; \
rabbitmqctl add_user noolitef noolitef 2>/dev/null ; \
rabbitmqctl add_user configStore configStore 2>/dev/null ; \
rabbitmqctl add_user client client 2>/dev/null ; \
rabbitmqctl add_vhost homecontrol ; \
rabbitmqctl set_permissions -p homecontrol configStore ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homecontrol controller ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homecontrol noolite ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homecontrol noolitef ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homecontrol client ".*" ".*" ".*" ; \
echo "*** Users completed. ***") &

# $@ is used to pass arguments to the rabbitmq-server command.
# For example if you use it like this: docker run -d rabbitmq arg1 arg2,
# it will be as you run in the container rabbitmq-server arg1 arg2
rabbitmq-server $@