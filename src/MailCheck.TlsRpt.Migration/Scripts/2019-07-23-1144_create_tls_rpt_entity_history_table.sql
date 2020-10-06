CREATE TABLE `tls_rpt_entity_history` (
  `id` VARCHAR(255) NOT NULL,
  `state` JSON NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;