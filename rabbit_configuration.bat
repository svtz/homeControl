cd C:\Program Files\RabbitMQ Server\rabbitmq_server-3.6.8\sbin

call rabbitmqctl add_user controller controller
call rabbitmqctl add_user noolite noolite
call rabbitmqctl add_user configStore configStore
call rabbitmqctl add_user client client

call rabbitmqctl add_vhost debug
call rabbitmqctl add_vhost release


call rabbitmqctl set_permissions -p debug configStore "$a" "^configuration$" "^configuration_requests$"
call rabbitmqctl set_permissions -p debug controller "$a" "^(configuration_requests|main)$" "^(configuration|main)$"
call rabbitmqctl set_permissions -p debug noolite "$a" "^(configuration_requests|main)$" "^(configuration|main)$"
call rabbitmqctl set_permissions -p debug client "$a" "^(configuration_requests|main)$" "^(configuration|main)$"


call rabbitmqctl set_permissions -p release configStore "$a" "^configuration$" "^configuration_requests$"
call rabbitmqctl set_permissions -p release controller "$a" "^(configuration_requests|main)$" "^(configuration|main)$"
call rabbitmqctl set_permissions -p release noolite "$a" "^(configuration_requests|main)$" "^(configuration|main)$"
call rabbitmqctl set_permissions -p release client "$a" "^(configuration_requests|main)$" "^(configuration|main)$"

pause