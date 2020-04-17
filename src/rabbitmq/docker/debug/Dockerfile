FROM rabbitmq:3.8-management

COPY src/rabbitmq/init-management.sh /init-management.sh
RUN ["chmod", "+x", "/init-management.sh"]

CMD ["/init-management.sh"]