#!/bin/sh

( sleep 60 ; \
rabbitmqctl add_user controller controller 2>/dev/null ; \
rabbitmqctl add_user noolite noolite 2>/dev/null ; \
rabbitmqctl add_user configStore configStore 2>/dev/null ; \
rabbitmqctl add_user client client 2>/dev/null ; \
rabbitmqctl add_vhost homeControl ; \
rabbitmqctl set_permissions -p homeControl configStore ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homeControl controller ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homeControl noolite ".*" ".*" ".*" ; \
rabbitmqctl set_permissions -p homeControl client ".*" ".*" ".*" ; \
echo "*** Users completed. ***") &

# $@ is used to pass arguments to the rabbitmq-server command.
# For example if you use it like this: docker run -d rabbitmq arg1 arg2,
# it will be as you run in the container rabbitmq-server arg1 arg2
rabbitmq-server $@