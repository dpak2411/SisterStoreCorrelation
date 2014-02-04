Feature: SisterStoreCorrelation
This Analytic App allows the user to assign a drivetime trade area for each location in a sample store file based on the surrounding population density and urban definition in order to perform a consumer segmentation analysis. 

Background:
  Given alteryx running at" http://gallery.alteryx.com/"
  And I am logged in using "deepak.manoharan@accionlabs.com" and "P@ssw0rd"

Scenario Outline:Run the analytical app
When I run the app "<app>" with the details of drive time trade areas "<Super Urban>","<Urban>","<Suburban>","<Exurban>","<Rural>"
And I specify the Potential Site Location "<Address>", "<City>", "<State>", "<Zip>"
Then I see the output contains the text <result>
Examples: 
| app                      | Super Urban | Urban | Suburban | Exurban | Rural | Address                   | City   | State | Zip     | result                                                |
| Sister Store Correlation | 10          | 12    | 18       | 25      | 30    | "230 Commerce, Suite 250" | Irvine | CA    | "92602" | "Sister Store Correlation"|