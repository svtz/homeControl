cd C:\Program Files\RabbitMQ Server\rabbitmq_server-3.6.8\sbin

call rabbitmqctl add_user controller controller
call rabbitmqctl add_user noolite noolite
call rabbitmqctl add_user configStore configStore
call rabbitmqctl add_user client client

call rabbitmqctl add_vhost debug
call rabbitmqctl add_vhost release


call rabbitmqctl set_permissions -p debug configStore ".*" ".*" ".*"
call rabbitmqctl set_permissions -p debug controller ".*" ".*" ".*"
call rabbitmqctl set_permissions -p debug noolite ".*" ".*" ".*"
call rabbitmqctl set_permissions -p debug client ".*" ".*" ".*"


call rabbitmqctl set_permissions -p release configStore ".*" ".*" ".*"
call rabbitmqctl set_permissions -p release controller ".*" ".*" ".*"
call rabbitmqctl set_permissions -p release noolite ".*" ".*" ".*"
call rabbitmqctl set_permissions -p release client ".*" ".*" ".*"

pause