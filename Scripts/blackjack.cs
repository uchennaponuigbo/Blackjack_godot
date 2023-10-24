using Godot;
using System;
using System.Linq;
using GC = Godot.Collections;

public partial class blackjack : Control
{
	//TODO
	//1. Implement Replay Button - DONE
	//2. Play Music - DONE
	//3. Visually Represent Cards in Player Hands
	private GC.Array cards = new GC.Array
	{ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10}; //jack, queen, king | last three numbers
	//1 Ace

	private const int MysteryCard = 0;
	GC.Array dealerHand = new GC.Array();
	GC.Array playerHand = new GC.Array();

	private int playerTotal = 0;
	private int dealerTotal = 0;

	private int playerWins = 0;
	private int dealerWins = 0;

	Button HitButton = null;
	Button StandButton = null;
	Button ReplayButton = null;

	Label dealerLabel = null;
	Label playerLabel = null;
	Label resultsLabel = null;
	Label labelPlayerWins = null;
	Label labelDealerWins = null;

	//for learning purposes of getting a reference of audio in code
	//for this case, it's card dealing sound effects
	AudioStreamPlayer2D audioStreamSFX = null;
	AudioStreamPlayer2D SFXWin = null;
	AudioStreamPlayer2D SFXLose = null;
	AudioStreamPlayer2D SFXDraw = null;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		HitButton = GetNode<Button>("HitButton");
		StandButton = GetNode<Button>("StandButton");
		ReplayButton = GetNode<Button>("ButtonReplay");

		dealerLabel = GetNode<Label>("DealerLabel");
		playerLabel = GetNode<Label>("PlayerLabel");
		resultsLabel = GetNode<Label>("ResultsLabel");

		labelPlayerWins = GetNode<Label>("LabelPlayerWins");
		labelDealerWins = GetNode<Label>("LabelDealerWins");

		audioStreamSFX = GetNode<AudioStreamPlayer2D>("SFXCard");
		SFXWin = GetNode<AudioStreamPlayer2D>("SFXWin");
		SFXLose = GetNode<AudioStreamPlayer2D>("SFXLose");
		SFXDraw = GetNode<AudioStreamPlayer2D>("SFXDraw");

		resultsLabel.Text = "";
		labelPlayerWins.Text = labelDealerWins.Text = $"Wins: {playerWins}";
		ReplayButton.Disabled = true;

		DealCards();
		CheckPlayer();

		GD.Print(playerHand);
		GD.Print(dealerHand);
	}

	public int Hit(GC.Array hand) 
	{
		audioStreamSFX.Play();
		int card = (int)cards.PickRandom();
		hand.Add(card);
		//audioStreamSFX.Finished	event
		return card;
	} 

	public void DealerDraw()
	{
		dealerHand.Remove(MysteryCard);
		Hit(dealerHand);
		dealerTotal = TotalCards(dealerHand);

		while(dealerTotal < 16)
		{
			Hit(dealerHand);
			dealerTotal = TotalCards(dealerHand);
		}

		dealerLabel.Text = $"Dealer Total: {dealerTotal}";
	}

	public void DealCards()
	{
		Hit(playerHand);
		Hit(dealerHand);

		Hit(playerHand);

		dealerTotal = TotalCards(dealerHand);
		playerTotal = TotalCards(playerHand);

		dealerHand.Add(MysteryCard);

		dealerLabel.Text = $"Dealer Total: {dealerTotal}";
		playerLabel.Text = $"Player Total: {playerTotal}";
	}

	public int TotalCards(GC.Array hand)
	{
		int total = 0;
		foreach(int cards in hand.Select(v => (int)v)) //explicit cast from Godot.Variant to int
		{
			total += cards;
		}
		return total;
	}

	public void CheckPlayer()
	{
		if(playerTotal > 21)
		{
			HitButton.Disabled = true;
			StandButton.Disabled = true;
			DeclareWinner();
		}
	}

	public void DeclareWinner()
	{
		HitButton.Disabled = true;
		StandButton.Disabled = true;
		ReplayButton.Disabled = false;
		
		if((playerTotal > dealerTotal && playerTotal <= 21) || dealerTotal > 21)
		{
			SFXWin.Play();
			playerWins++;
			//GD.Print("Player Wins!");
			labelPlayerWins.Text = $"Wins: {playerWins}";
			resultsLabel.Text = "Player Wins!";
		}
		else if((playerTotal < dealerTotal && dealerTotal <= 21) || playerTotal > 21)
		{
			SFXLose.Play();
			dealerWins++;
			//GD.Print("Player Loses...");
			labelDealerWins.Text = $"Wins: {dealerWins}";
			resultsLabel.Text = "Player Loses...";
		}
		else if(playerTotal == dealerTotal)
		{
			SFXDraw.Play();
			playerWins++;
			dealerWins++;
			//GD.Print("It's a Draw.");
			resultsLabel.Text = "It's a Draw.";
		}
	}

	public void ClearDecks()
	{
		playerHand.Clear();
		dealerHand.Clear();
		resultsLabel.Text = "";
	}

	//Connecting a signal on the Editor requires the name of the function
	//Scene > Click Node > Go to Inspector on right and click 'Node' > Scroll through signals and click the one you want
	public void _on_hit_button_pressed()
	{
		//GD.PrintS("Debug", "Player Total", playerTotal);
		Hit(playerHand);
		playerTotal = TotalCards(playerHand);
		playerLabel.Text = $"Player Total: {playerTotal}";
		CheckPlayer();
	}

	public void _on_stand_button_pressed()
	{
		//GD.PrintS("Debug", "Dealer Total", dealerTotal);
		DealerDraw();
		DeclareWinner();
	}

	public void _on_button_replay_pressed()
	{
		ClearDecks();
		DealCards();
		CheckPlayer();

		ReplayButton.Disabled = true;
		HitButton.Disabled = false;
		StandButton.Disabled = false;

		GD.Print(playerHand);
		GD.Print(dealerHand);
	}

	//Programmatically connect a signal
	private void ConnectSignalToNode() //button
	{
		//connect to 'this' object with a script attached to it, name of function
		HitButton.Connect("pressed", new Callable(this, "HitPressed")); 
		HitButton.Connect("pressed", new Callable(this, "StandPressed")); 
		//Call the "HitPressed" method in the _Ready() Method
	}

	public void HitPressed()
	{
		GD.PrintS("Debug: Programmed Signal", "Player Total", playerTotal);
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
