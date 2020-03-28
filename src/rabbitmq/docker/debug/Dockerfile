FROM rabbitmq:3.8-management

COPY src/rabbitmq/init.sh /init.sh
RUN ["chmod", "+x", "/init.sh"]

CMD ["/init-management.sh"]