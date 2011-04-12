Wcf helper classes
==================

A library of Wcf helper classes.

Example of usage
-----------------

	public void SomeMethod()
	{
	    // Creates the channel for the IService webservice
		var client = ClientManager.CreateChannel<IService>();
		client.DoStuff();
	}


Is that it?
--------------------------

Of course no..

* If no endpoint is found for the contract (IService), an exception is thrown
* If more than one endpoint is found, an exception is throwm
* CreateChannel takes a bindingnam or bindingtype as an argument in the case you have more than one enpoint for the contract.
* If the service is a federated service, the securitytoken is cached so you can reuse it. Saves a roundtrip to the STS every time you are using the service.
* Extension methods to the ChannelFactory (IsFederated, IsConfiguredAsFederated, SetClientCredentials)
